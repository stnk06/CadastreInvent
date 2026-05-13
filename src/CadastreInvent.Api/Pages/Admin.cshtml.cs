using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Shared.Application.Admin.Commands;
using CadastreInvent.Shared.Application.Auth;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Api.Pages
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminModel : PageModel
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IMediator _mediator;

        public AdminModel(CadastreDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "users";

        [TempData]
        public string StatusMessage { get; set; }

        public class UserViewModel
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string RoleName { get; set; }
            public Guid RoleId { get; set; }
            public bool IsActive { get; set; }
        }

        public List<UserViewModel> UsersList { get; set; } = new();
        public List<Role> RolesList { get; set; } = new();
        public List<AuditLog> AuditLogsList { get; set; } = new();
        public Dictionary<string, string> PermissionDescriptions { get; set; } = new();

        [BindProperty] public CreateUserCommand NewUser { get; set; }
        [BindProperty] public Guid ChangeRoleUserId { get; set; }
        [BindProperty] public Guid ChangeRoleNewRoleId { get; set; }
        [BindProperty] public Guid ChangePasswordUserId { get; set; }
        [BindProperty] public string NewPassword { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(Tab)) Tab = "users";

            RolesList = await _dbContext.Roles.Include(r => r.Permissions).OrderBy(r => r.Name).ToListAsync();

            PermissionDescriptions = new Dictionary<string, string>
            {
                { Permissions.AdminAccess, "Доступ к панели администрирования" },
                { Permissions.DataDelete, "Физическое удаление данных (Корзина)" },
                { Permissions.CreateSpatialUnit, "Создание пространственных контуров (ГИС)" },
                { Permissions.UpdateBoundary, "Редактирование границ объектов (ГИС)" },
                { Permissions.ManageBAUnits, "Ведение реестра объектов недвижимости" },
                { Permissions.ManageParties, "Ведение реестра субъектов (лиц)" },
                { Permissions.RegisterRRR, "Регистрация прав, сделок и ограничений" },
                { Permissions.TerminateRRR, "Прекращение (погашение) прав" },
                { Permissions.CreateValuationUnit, "Постановка объектов на оценку (CAMA)" },
                { Permissions.RegisterTransaction, "Ведение реестра рыночных цен (Сделки)" },
                { Permissions.ManageAppeals, "Рассмотрение апелляций и споров" },
                { Permissions.ManageFieldTasks, "Диспетчеризация полевых инспекций" },
                { Permissions.ExecuteFieldTasks, "Доступ к мобильному терминалу инспектора" },
                { Permissions.ViewGisMap, "Просмотр и анализ ГИС Карты" }
            };

            if (Tab == "users")
            {
                var users = await _dbContext.Users.AsNoTracking().ToListAsync();
                var roleDict = RolesList.ToDictionary(r => r.Id, r => r.Name);
                UsersList = users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = roleDict.ContainsKey(u.RoleId) ? roleDict[u.RoleId] : "Неизвестно",
                    IsActive = u.IsActive
                }).OrderBy(u => u.Username).ToList();
            }
            else if (Tab == "audit")
            {
                AuditLogsList = await _dbContext.AuditLogs.AsNoTracking().OrderByDescending(a => a.Timestamp).Take(200).ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostUpdateMatrixAsync(List<string> SelectedPermissions)
        {
            var roles = await _dbContext.Roles.Include(r => r.Permissions).ToListAsync();
            foreach (var role in roles)
            {
                role.ClearPermissions();
                var rolePrefix = role.Id.ToString() + "|";
                var permissionsForRole = SelectedPermissions.Where(x => x.StartsWith(rolePrefix)).Select(x => x.Replace(rolePrefix, ""));
                foreach (var p in permissionsForRole)
                {
                    role.AddPermission(p);
                }
            }
            await _dbContext.SaveChangesAsync();
            StatusMessage = "Матрица доступов успешно обновлена для всей организации.";
            return RedirectToPage(new { tab = "roles" });
        }

        public async Task<IActionResult> OnPostCreateUserAsync()
        {
            try { await _mediator.Send(NewUser); StatusMessage = "Сотрудник зарегистрирован."; }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
            return RedirectToPage(new { tab = "users" });
        }

        public async Task<IActionResult> OnPostChangeRoleAsync()
        {
            try { await _mediator.Send(new ChangeUserRoleCommand(ChangeRoleUserId, ChangeRoleNewRoleId)); StatusMessage = "Роль изменена."; }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
            return RedirectToPage(new { tab = "users" });
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid userId, bool activate)
        {
            try { await _mediator.Send(new ToggleUserStatusCommand(userId, activate)); StatusMessage = "Статус обновлен."; }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
            return RedirectToPage(new { tab = "users" });
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            try { await _mediator.Send(new ChangeUserPasswordCommand(ChangePasswordUserId, NewPassword)); StatusMessage = "Пароль изменен."; }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
            return RedirectToPage(new { tab = "users" });
        }

        // ==========================================
        // ==========================================

        public async Task<IActionResult> OnPostWipeDatabaseAsync()
        {
            try
            {
                var sql = @"
                DO $$ 
                DECLARE 
                    r RECORD; 
                BEGIN 
                    FOR r IN 
                        SELECT schemaname, tablename 
                        FROM pg_tables 
                        WHERE schemaname IN ('registry', 'valuation', 'inspection') 
                    LOOP 
                        EXECUTE format('TRUNCATE TABLE %I.%I CASCADE;', r.schemaname, r.tablename); 
                    END LOOP; 
                END; 
                $$;";

                await _dbContext.Database.ExecuteSqlRawAsync(sql);
                await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE shared.audit_logs CASCADE; TRUNCATE TABLE shared.event_streams CASCADE;");

                StatusMessage = "База данных успешно очищена.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Сбой при очистке: {ex.Message}";
            }
            return RedirectToPage(new { tab = "database" });
        }

        public async Task<IActionResult> OnPostExportBackupAsync()
        {
            try
            {
                var snapshot = new
                {
                    ExportDateUtc = DateTime.UtcNow,
                    SystemVersion = "CadastreInvent 1.0",
                    SpatialUnits = await _dbContext.SpatialUnits.AsNoTracking().Select(s => new { s.Id, s.ReferenceNumber, s.Type, s.AreaSqMeters, Wkt = s.Boundary.AsText() }).ToListAsync(),
                    BAUnits = await _dbContext.BAUnits.AsNoTracking().Select(b => new { b.Id, b.Name, b.Type }).ToListAsync(),
                    Parties = await _dbContext.Parties.AsNoTracking().Select(p => new { p.Id, p.ExtId, p.Name, p.Type, p.ContactInfo }).ToListAsync(),
                    ValuationUnits = await _dbContext.ValuationUnits.AsNoTracking().Select(v => new { v.Id, v.BAUnitId, v.ZoningStatus }).ToListAsync(),
                    Characteristics = await _dbContext.PropertyCharacteristics.AsNoTracking().Select(c => new { c.ValuationUnitId, c.CharacteristicsJson }).ToListAsync()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(snapshot, options));

                return File(jsonBytes, "application/json", $"Cadastre_Snapshot_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка резервного копирования: {ex.Message}";
                return RedirectToPage(new { tab = "database" });
            }
        }

        // ==========================================
        // ==========================================

        public async Task<IActionResult> OnPostGenerateRegistrySeedAsync()
        {
            try
            {
                var rand = new Random();
                int targetCount = rand.Next(100, 201);
                var fac = new GeometryFactory(new PrecisionModel(), 4326);

                double baseLat = 55.75;
                double baseLon = 37.61;

                string[] streets = { "Тверская ул.", "Пресненская наб.", "Кутузовский пр-т", "пр-т Мира", "ул. Остоженка", "Садовое кольцо", "ул. Арбат", "Ленинградский пр-т", "ул. Петровка", "Цветной б-р" };
                string[] corpNames = { "ООО 'СтройИнвест'", "АО 'Монолит'", "ПАО 'Галс'", "ООО 'Капитал'", "АО 'ГлавСтрой'" };
                string[] personNames = { "Иванов А.А.", "Петров В.С.", "Смирнова Е.И.", "Соколов Д.М.", "Волкова А.В.", "Михайлов Н.Н.", "Кузнецова О.П." };

                var spatialUnits = new List<SpatialUnit>();
                var baUnits = new List<BAUnit>();
                var parties = new List<Party>();
                var sources = new List<Source>();
                var rrrs = new List<RRR>();

                var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _dbContext.Database.BeginTransactionAsync();

                    for (int i = 0; i < targetCount; i++)
                    {
                        double offsetLat = (rand.NextDouble() - 0.5) * 0.3;
                        double offsetLon = (rand.NextDouble() - 0.5) * 0.3;
                        double size = 0.001 + (rand.NextDouble() * 0.002);

                        var coords = new[] {
                            new Coordinate(baseLon + offsetLon, baseLat + offsetLat),
                            new Coordinate(baseLon + offsetLon, baseLat + offsetLat + size),
                            new Coordinate(baseLon + offsetLon + size, baseLat + offsetLat + size),
                            new Coordinate(baseLon + offsetLon + size, baseLat + offsetLat),
                            new Coordinate(baseLon + offsetLon, baseLat + offsetLat)
                        };
                        var poly = fac.CreatePolygon(coords);

                        double fakeAreaSqMeters = Math.Round(rand.NextDouble() * 4000 + 500, 1);
                        string cadNum = $"77:01:{rand.Next(1000000, 9999999)}:{rand.Next(1000, 9999)}";
                        var sType = rand.Next(10) > 2 ? SpatialUnitType.Parcel : SpatialUnitType.Building;

                        var su = new SpatialUnit(cadNum, sType, poly, fakeAreaSqMeters);
                        spatialUnits.Add(su);

                        string address = $"г. Москва, {streets[rand.Next(streets.Length)]}, д. {rand.Next(1, 150)}";
                        var ba = new BAUnit(address, BAUnitType.BasicPropertyUnit);
                        ba.AddSpatialUnit(su.Id);
                        baUnits.Add(ba);

                        bool isCorp = rand.Next(10) > 6;
                        string ownerName = isCorp ? corpNames[rand.Next(corpNames.Length)] : personNames[rand.Next(personNames.Length)];
                        var partyType = isCorp ? PartyType.NonNaturalPerson : PartyType.NaturalPerson;
                        var extId = isCorp ? $"{rand.Next(1000000000, 2147483647)}" : $"{rand.Next(100000000, 999999999)}{rand.Next(100, 999)}";

                        var party = new Party(extId, ownerName, partyType, "+7 (999) " + rand.Next(1000000, 9999999));
                        parties.Add(party);

                        var src = new Source(SourceType.SaleContract, $"ДКП-{rand.Next(1000, 9999)}", DateTime.UtcNow.AddDays(-rand.Next(30, 2000)), "");
                        sources.Add(src);

                        var rrr = new RRR(RRRType.Ownership, ba.Id, src.Id, 1, 1, src.RecordDate, party.Id, null);
                        rrrs.Add(rrr);
                    }

                    _dbContext.SpatialUnits.AddRange(spatialUnits);
                    _dbContext.BAUnits.AddRange(baUnits);
                    _dbContext.Parties.AddRange(parties);
                    _dbContext.Sources.AddRange(sources);
                    _dbContext.Rrrs.AddRange(rrrs);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                });

                return new JsonResult(new { success = true, count = targetCount });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostGenerateValuationSeedAsync()
        {
            try
            {
                var rand = new Random();
                int targetCount = rand.Next(20, 31);

                // Находим объекты, которые еще не поставлены на оценку
                var unassessedBaUnits = await _dbContext.BAUnits
                    .Include(b => b.SpatialUnits)
                    .Where(b => !_dbContext.ValuationUnits.Any(v => v.BAUnitId == b.Id) && b.SpatialUnits.Any())
                    .Take(500)
                    .ToListAsync();

                if (!unassessedBaUnits.Any())
                    return new JsonResult(new { success = false, error = "Свободные объекты в ЕГРН отсутствуют. Сначала сгенерируйте реестр." });

                // Перемешиваем и берем 20-30 штук
                unassessedBaUnits = unassessedBaUnits.OrderBy(x => Guid.NewGuid()).Take(targetCount).ToList();

                var spatialUnitIds = unassessedBaUnits.SelectMany(b => b.SpatialUnits.Select(su => su.SpatialUnitId)).ToList();
                var spatialUnits = await _dbContext.SpatialUnits.Where(s => spatialUnitIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

                string[] zonings = { "Residential", "Commercial", "Industrial", "Agricultural" };

                var valUnits = new List<ValuationUnit>();
                var characteristics = new List<PropertyCharacteristic>();
                var salesTransactions = new List<SalesTransaction>();

                var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _dbContext.Database.BeginTransactionAsync();

                    foreach (var ba in unassessedBaUnits)
                    {
                        string zone = zonings[rand.Next(zonings.Length)];
                        var vu = new ValuationUnit(ba.Id, zone);
                        valUnits.Add(vu);

                        double fakeAreaSqMeters = 100;
                        var suLink = ba.SpatialUnits.FirstOrDefault();
                        if (suLink != null && spatialUnits.TryGetValue(suLink.SpatialUnitId, out var su))
                        {
                            fakeAreaSqMeters = su.AreaSqMeters;
                        }

                        int yearBuilt = rand.Next(1960, 2025);
                        int distance = rand.Next(1, 30);
                        int floor = rand.Next(1, 15);
                        int rooms = rand.Next(1, 6);

                        var charsObj = new
                        {
                            Area = fakeAreaSqMeters,
                            YearBuilt = yearBuilt,
                            Floor = floor,
                            DistanceToCenterKm = distance,
                            RoomsCount = rooms
                        };
                        var charEntity = new PropertyCharacteristic(vu.Id, JsonSerializer.Serialize(charsObj));
                        characteristics.Add(charEntity);

                        // Строгая линейная формула для успешного обучения ML
                        decimal basePricePerSqM = zone == "Commercial" ? 250000m : zone == "Residential" ? 150000m : 80000m;
                        decimal price = basePricePerSqM * (decimal)fakeAreaSqMeters;

                        price *= (1m - ((decimal)distance * 0.015m));
                        price *= (1m + ((yearBuilt - 1950) * 0.003m));
                        price *= (1m + (floor * 0.005m));

                        if (price < 500000m) price = 500000m;

                        // Ограниченный рыночный шум +/- 4%
                        price *= (decimal)(1.0 + (rand.NextDouble() * 0.08 - 0.04));
                        price = Math.Round(price / 1000m) * 1000m;

                        var saleDate = DateTime.UtcNow.AddDays(-rand.Next(1, 365));
                        var sale = new SalesTransaction(vu.Id, price, saleDate, TransactionValidity.ValidMarket);
                        salesTransactions.Add(sale);
                    }

                    _dbContext.ValuationUnits.AddRange(valUnits);
                    _dbContext.PropertyCharacteristics.AddRange(characteristics);
                    _dbContext.SalesTransactions.AddRange(salesTransactions);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                });

                return new JsonResult(new { success = true, count = unassessedBaUnits.Count });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }
}