using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace CadastreInvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "valuation");

            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.EnsureSchema(
                name: "registry");

            migrationBuilder.EnsureSchema(
                name: "inspection");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "appraisal_results",
                schema: "valuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpatialUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculatedValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ConfidenceScore = table.Column<float>(type: "real", nullable: false),
                    AppraisalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MlModelVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appraisal_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangesJson = table.Column<string>(type: "jsonb", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ba_units",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ba_units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "batch_registration_jobs",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batch_registration_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event_streams",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_streams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inspection_observations",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    RemarksJson = table.Column<string>(type: "jsonb", nullable: false),
                    ObservationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AppLocalId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection_observations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mass_appraisal_models",
                schema: "valuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Algorithm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TrainingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModelData = table.Column<byte[]>(type: "bytea", nullable: true),
                    MetricsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mass_appraisal_models", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parties",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ContactInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "party_groups",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sources",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContentUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "spatial_units",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AreaSqMeters = table.Column<double>(type: "double precision", nullable: false),
                    Boundary = table.Column<Geometry>(type: "geometry(Polygon, 4326)", nullable: false),
                    Srid = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spatial_units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rrrs",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    BAUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartyGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShareNumerator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ShareDenominator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rrrs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rrrs_ba_units_BAUnitId",
                        column: x => x.BAUnitId,
                        principalSchema: "registry",
                        principalTable: "ba_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "valuation_units",
                schema: "valuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BAUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoningStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_valuation_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_valuation_units_ba_units_BAUnitId",
                        column: x => x.BAUnitId,
                        principalSchema: "registry",
                        principalTable: "ba_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "batch_registration_items",
                schema: "registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Wkt = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ExtId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batch_registration_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_batch_registration_items_batch_registration_jobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "registry",
                        principalTable: "batch_registration_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_group_members",
                schema: "registry",
                columns: table => new
                {
                    PartyGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShareNumerator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ShareDenominator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_group_members", x => new { x.PartyGroupId, x.PartyId });
                    table.ForeignKey(
                        name: "FK_party_group_members_party_groups_PartyGroupId",
                        column: x => x.PartyGroupId,
                        principalSchema: "registry",
                        principalTable: "party_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermission_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "shared",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "shared",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ba_unit_spatial_units",
                schema: "registry",
                columns: table => new
                {
                    BAUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpatialUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ba_unit_spatial_units", x => new { x.BAUnitId, x.SpatialUnitId });
                    table.ForeignKey(
                        name: "FK_ba_unit_spatial_units_ba_units_BAUnitId",
                        column: x => x.BAUnitId,
                        principalSchema: "registry",
                        principalTable: "ba_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ba_unit_spatial_units_spatial_units_SpatialUnitId",
                        column: x => x.SpatialUnitId,
                        principalSchema: "registry",
                        principalTable: "spatial_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "property_characteristics",
                schema: "valuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValuationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacteristicsJson = table.Column<string>(type: "jsonb", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_characteristics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_property_characteristics_valuation_units_ValuationUnitId",
                        column: x => x.ValuationUnitId,
                        principalSchema: "valuation",
                        principalTable: "valuation_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_transactions",
                schema: "valuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValuationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Validity = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_transactions_valuation_units_ValuationUnitId",
                        column: x => x.ValuationUnitId,
                        principalSchema: "valuation",
                        principalTable: "valuation_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "valuations",
                schema: "valuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValuationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessedValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValuationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_valuations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_valuations_mass_appraisal_models_ModelId",
                        column: x => x.ModelId,
                        principalSchema: "valuation",
                        principalTable: "mass_appraisal_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_valuations_valuation_units_ValuationUnitId",
                        column: x => x.ValuationUnitId,
                        principalSchema: "valuation",
                        principalTable: "valuation_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inspection_tasks",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetSpatialUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetCoordinates = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedInspectorId = table.Column<Guid>(type: "uuid", nullable: true),
                    State = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inspection_tasks_spatial_units_TargetSpatialUnitId",
                        column: x => x.TargetSpatialUnitId,
                        principalSchema: "registry",
                        principalTable: "spatial_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inspection_tasks_users_AssignedInspectorId",
                        column: x => x.AssignedInspectorId,
                        principalSchema: "shared",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "valuation_appeals",
                schema: "valuation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValuationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_valuation_appeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_valuation_appeals_parties_ApplicantPartyId",
                        column: x => x.ApplicantPartyId,
                        principalSchema: "registry",
                        principalTable: "parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_valuation_appeals_valuations_ValuationId",
                        column: x => x.ValuationId,
                        principalSchema: "valuation",
                        principalTable: "valuations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inspection_results",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Conclusion = table.Column<string>(type: "text", nullable: false),
                    RecordedCoordinates = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    HasGpsDiscrepancy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inspection_results_inspection_tasks_InspectionTaskId",
                        column: x => x.InspectionTaskId,
                        principalSchema: "inspection",
                        principalTable: "inspection_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inspection_photos",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AppLocalId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamp with time zone")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection_photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inspection_photos_inspection_results_InspectionResultId",
                        column: x => x.InspectionResultId,
                        principalSchema: "inspection",
                        principalTable: "inspection_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appraisal_results_SpatialUnitId",
                schema: "valuation",
                table: "appraisal_results",
                column: "SpatialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityId",
                schema: "shared",
                table: "audit_logs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ba_unit_spatial_units_SpatialUnitId",
                schema: "registry",
                table: "ba_unit_spatial_units",
                column: "SpatialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_batch_registration_items_JobId",
                schema: "registry",
                table: "batch_registration_items",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_batch_registration_items_Status",
                schema: "registry",
                table: "batch_registration_items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_event_streams_AggregateId_Version",
                schema: "shared",
                table: "event_streams",
                columns: new[] { "AggregateId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inspection_observations_AppLocalId",
                schema: "inspection",
                table: "inspection_observations",
                column: "AppLocalId",
                unique: true,
                filter: "\"AppLocalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_observations_InspectionTaskId",
                schema: "inspection",
                table: "inspection_observations",
                column: "InspectionTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_photos_AppLocalId",
                schema: "inspection",
                table: "inspection_photos",
                column: "AppLocalId",
                unique: true,
                filter: "\"AppLocalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_photos_InspectionResultId",
                schema: "inspection",
                table: "inspection_photos",
                column: "InspectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_results_InspectionTaskId",
                schema: "inspection",
                table: "inspection_results",
                column: "InspectionTaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inspection_results_RecordedCoordinates",
                schema: "inspection",
                table: "inspection_results",
                column: "RecordedCoordinates")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_tasks_AssignedInspectorId",
                schema: "inspection",
                table: "inspection_tasks",
                column: "AssignedInspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_tasks_TargetCoordinates",
                schema: "inspection",
                table: "inspection_tasks",
                column: "TargetCoordinates")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_tasks_TargetSpatialUnitId",
                schema: "inspection",
                table: "inspection_tasks",
                column: "TargetSpatialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_property_characteristics_CharacteristicsJson",
                schema: "valuation",
                table: "property_characteristics",
                column: "CharacteristicsJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_property_characteristics_ValuationUnitId",
                schema: "valuation",
                table: "property_characteristics",
                column: "ValuationUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleId",
                schema: "shared",
                table: "RolePermission",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_rrrs_BAUnitId",
                schema: "registry",
                table: "rrrs",
                column: "BAUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_rrrs_PartyId",
                schema: "registry",
                table: "rrrs",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_transactions_ValuationUnitId",
                schema: "valuation",
                table: "sales_transactions",
                column: "ValuationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_spatial_units_Boundary",
                schema: "registry",
                table: "spatial_units",
                column: "Boundary")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_spatial_units_ReferenceNumber",
                schema: "registry",
                table: "spatial_units",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "shared",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                schema: "shared",
                table: "users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_valuation_appeals_ApplicantPartyId",
                schema: "valuation",
                table: "valuation_appeals",
                column: "ApplicantPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_valuation_appeals_ValuationId",
                schema: "valuation",
                table: "valuation_appeals",
                column: "ValuationId");

            migrationBuilder.CreateIndex(
                name: "IX_valuation_units_BAUnitId",
                schema: "valuation",
                table: "valuation_units",
                column: "BAUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_valuations_ModelId",
                schema: "valuation",
                table: "valuations",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_valuations_ValuationUnitId",
                schema: "valuation",
                table: "valuations",
                column: "ValuationUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appraisal_results",
                schema: "valuation");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "ba_unit_spatial_units",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "batch_registration_items",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "event_streams",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "inspection_observations",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "inspection_photos",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "party_group_members",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "property_characteristics",
                schema: "valuation");

            migrationBuilder.DropTable(
                name: "RolePermission",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "rrrs",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "sales_transactions",
                schema: "valuation");

            migrationBuilder.DropTable(
                name: "sources",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "valuation_appeals",
                schema: "valuation");

            migrationBuilder.DropTable(
                name: "batch_registration_jobs",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "inspection_results",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "party_groups",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "parties",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "valuations",
                schema: "valuation");

            migrationBuilder.DropTable(
                name: "inspection_tasks",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "mass_appraisal_models",
                schema: "valuation");

            migrationBuilder.DropTable(
                name: "valuation_units",
                schema: "valuation");

            migrationBuilder.DropTable(
                name: "spatial_units",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "users",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "ba_units",
                schema: "registry");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "shared");
        }
    }
}
