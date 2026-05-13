using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using CadastreInvent.Inspection.Domain.Entities;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Infrastructure.Persistence
{
    public class CadastreDbContext : DbContext
    {
        private static bool _historyTablesSynced = false;
        private readonly ICurrentUserService _currentUserService;

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EventStream> EventStreams { get; set; }

        public DbSet<SpatialUnit> SpatialUnits { get; set; }
        public DbSet<BAUnit> BAUnits { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<PartyGroup> PartyGroups { get; set; }
        public DbSet<RRR> Rrrs { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<BatchRegistrationJob> BatchRegistrationJobs { get; set; }
        public DbSet<BatchRegistrationItem> BatchRegistrationItems { get; set; }
        public DbSet<ImportHistory> ImportHistories { get; set; }

        public DbSet<ValuationUnit> ValuationUnits { get; set; }
        public DbSet<PropertyCharacteristic> PropertyCharacteristics { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<MassAppraisalModel> MassAppraisalModels { get; set; }
        public DbSet<CadastreInvent.Valuation.Domain.Entities.Valuation> Valuations { get; set; }
        public DbSet<ValuationAppeal> ValuationAppeals { get; set; }

        public DbSet<InspectionTask> InspectionTasks { get; set; }
        public DbSet<InspectionPhoto> InspectionPhotos { get; set; }
        public DbSet<InspectionObservation> InspectionObservations { get; set; }

        public CadastreDbContext(DbContextOptions<CadastreDbContext> options, ICurrentUserService currentUserService = null) : base(options)
        {
            _currentUserService = currentUserService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis");
            modelBuilder.HasDefaultSchema("shared");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CadastreDbContext).Assembly);

            modelBuilder.Entity<ImportHistory>(entity =>
            {
                entity.ToTable("import_histories", "registry");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ImportDateUtc).HasDefaultValueSql("now()");
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(CadastreInvent.Shared.Domain.Entities.DomainEntity).IsAssignableFrom(entityType.ClrType) ||
                    typeof(CadastreInvent.Registry.Domain.Entities.DomainEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Property<bool>("IsDeleted").HasDefaultValue(false);
                    modelBuilder.Entity(entityType.ClrType).Property<DateTime?>("DeletedAt");

                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
                    var isDeletedPropExp = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));
                    var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedPropExp, Expression.Constant(false));
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("ValidFrom")
                        .HasDefaultValueSql("now()")
                        .ValueGeneratedOnAddOrUpdate();

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("ValidTo")
                        .HasDefaultValueSql("'infinity'::timestamp with time zone")
                        .ValueGeneratedOnAddOrUpdate();
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (!_historyTablesSynced)
            {
                try
                {
                    var sql = @"
                    DO $EF$
                    DECLARE
                        s text;
                        t text;
                    BEGIN
                        FOR s, t IN
                            SELECT table_schema, table_name
                            FROM information_schema.tables
                            WHERE table_schema IN ('registry', 'valuation', 'inspection', 'shared')
                              AND table_type = 'BASE TABLE'
                              AND table_name NOT LIKE '%_history'
                              AND table_name NOT IN ('audit_logs', 'event_streams', '__EFMigrationsHistory', 'RolePermission', 'party_group_members', 'ba_unit_spatial_units')
                        LOOP
                            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = s AND table_name = t || '_history') THEN
                                EXECUTE format('DROP TABLE %I.%I CASCADE', s, t || '_history');
                                EXECUTE format('CREATE TABLE %I.%I (LIKE %I.%I)', s, t || '_history', s, t);
                            END IF;
                        END LOOP;
                    END $EF$;";
                    await Database.ExecuteSqlRawAsync(sql, cancellationToken);
                }
                catch { }
                finally { _historyTablesSynced = true; }
            }

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            var userId = _currentUserService?.UserId ?? Guid.Empty;

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType().Name;

                if (entityType == nameof(EventStream) ||
                    entityType == nameof(AuditLog) ||
                    entityType == nameof(MassAppraisalModel) ||
                    entityType == nameof(BatchRegistrationItem) ||
                    entityType == nameof(ImportHistory))
                    continue;

                var idProperty = entry.Entity.GetType().GetProperty("Id");
                if (idProperty == null || idProperty.PropertyType != typeof(Guid))
                {
                    continue;
                }

                Guid entityId = (Guid)idProperty.GetValue(entry.Entity);
                var intendedState = entry.State.ToString();

                if (entry.State == EntityState.Deleted && entry.Metadata.FindProperty("IsDeleted") != null)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                    entry.Property("DeletedAt").CurrentValue = DateTime.UtcNow;
                }

                var changes = new System.Collections.Generic.Dictionary<string, object>();
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    foreach (var property in entry.Properties.Where(p => p.IsModified || entry.State == EntityState.Added))
                    {
                        if (property.Metadata.ClrType == typeof(byte[])) continue;

                        changes[property.Metadata.Name] = new { Old = property.OriginalValue, New = property.CurrentValue };
                    }
                }

                var currentVersion = EventStreams
                    .Where(e => e.AggregateId == entityId)
                    .Select(e => (int?)e.Version)
                    .Max() ?? 0;

                var jsonOptions = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
                jsonOptions.Converters.Add(new GeometryJsonConverterFactory());

                var domainEvent = new EventStream(
                    aggregateId: entityId,
                    aggregateType: entityType,
                    eventType: $"{entityType}{intendedState}",
                    eventDataJson: JsonSerializer.Serialize(entry.Entity, jsonOptions),
                    version: currentVersion + 1
                );

                var auditLog = new AuditLog(
                    entityId: entityId,
                    entityName: entityType,
                    action: intendedState,
                    changesJson: JsonSerializer.Serialize(changes, jsonOptions),
                    userId: userId
                );

                EventStreams.Add(domainEvent);
                AuditLogs.Add(auditLog);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

    public class GeometryJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeof(Geometry).IsAssignableFrom(typeToConvert);
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new GeometryJsonConverter();

        private class GeometryJsonConverter : JsonConverter<Geometry>
        {
            public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null!;
            public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
            {
                if (value == null) writer.WriteNullValue();
                else writer.WriteStringValue(value.AsText());
            }
        }
    }
}