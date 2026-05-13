using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Infrastructure.Persistence.Extensions;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Shared.Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CadastreInvent.Api.Pages
{
    [Authorize]
    public class RegistryModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly CadastreDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public RegistryModel(IMediator mediator, CadastreDbContext dbContext, IWebHostEnvironment environment)
        {
            _mediator = mediator;
            _dbContext = dbContext;
            _environment = environment;
        }

        [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "baunits";
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public DateTime? AsOfDate { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterType { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FilterDateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FilterDateTo { get; set; }

        public int PageSize { get; set; } = 25;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));

        [TempData] public string StatusMessage { get; set; }

        public class BAUnitViewModel { public Guid Id { get; set; } public string Name { get; set; } public string TypeStr { get; set; } public bool IsDeleted { get; set; } public List<string> SpatialUnitReferences { get; set; } = new(); public Dictionary<Guid, string> SpatialUnitIdsMap { get; set; } = new(); }
        public class RrrViewModel { public Guid Id { get; set; } public string TypeStr { get; set; } public Guid BAUnitId { get; set; } public string BAUnitName { get; set; } public string SubjectName { get; set; } public string SourceName { get; set; } public decimal ShareNumerator { get; set; } public decimal ShareDenominator { get; set; } public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } public bool IsDeleted { get; set; } }
        public class PartyGroupMemberViewModel { public Guid PartyId { get; set; } public string PartyName { get; set; } public decimal ShareNumerator { get; set; } public decimal ShareDenominator { get; set; } }
        public class PartyGroupViewModel { public Guid Id { get; set; } public string Name { get; set; } public bool IsDeleted { get; set; } public List<PartyGroupMemberViewModel> Members { get; set; } = new(); }
        public class SpatialUnitViewModel { public Guid Id { get; set; } public string ReferenceNumber { get; set; } public string TypeStr { get; set; } public double AreaSqMeters { get; set; } public bool IsDeleted { get; set; } }
        public class PartyViewModel { public Guid Id { get; set; } public string ExtId { get; set; } public string Name { get; set; } public string TypeStr { get; set; } public string ContactInfo { get; set; } public bool IsDeleted { get; set; } }
        public class SourceViewModel { public Guid Id { get; set; } public string TypeStr { get; set; } public string DocumentNumber { get; set; } public DateTime RecordDate { get; set; } public string ContentUrl { get; set; } public bool IsDeleted { get; set; } }

        public List<BAUnitViewModel> BAUnits { get; set; } = new();
        public List<PartyViewModel> Parties { get; set; } = new();
        public List<RrrViewModel> Rrrs { get; set; } = new();
        public List<SourceViewModel> Sources { get; set; } = new();
        public List<PartyGroupViewModel> PartyGroups { get; set; } = new();
        public List<SpatialUnitViewModel> SpatialUnits { get; set; } = new();

        [BindProperty] public CreateBAUnitCommand BAUnitInput { get; set; }
        [BindProperty] public RegisterPartyCommand PartyInput { get; set; }
        [BindProperty] public RegisterRRRCommand RRRInput { get; set; }
        [BindProperty] public string RRRSubjectType { get; set; } = "Individual";
        [BindProperty] public CreateSourceCommand SourceInput { get; set; }
        [BindProperty] public CreatePartyGroupCommand PartyGroupInput { get; set; }
        [BindProperty] public AddPartyToGroupCommand AddPartyToGroupInput { get; set; }
        [BindProperty] public TerminateRRRCommand TerminateRRRInput { get; set; }
        [BindProperty] public AddSpatialUnitToBAUnitCommand AddSpatialUnitInput { get; set; }
        [BindProperty] public IFormFile UploadedFile { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(Tab)) Tab = "baunits";
            if (PageNumber < 1) PageNumber = 1;
            int skip = (PageNumber - 1) * PageSize;

            IQueryable<BAUnit> baQuery = _dbContext.BAUnits.AsNoTracking();
            IQueryable<Party> partyQuery = _dbContext.Parties.AsNoTracking();
            IQueryable<RRR> rrrQuery = _dbContext.Rrrs.AsNoTracking();
            IQueryable<Source> sourceQuery = _dbContext.Sources.AsNoTracking();
            IQueryable<PartyGroup> groupQuery = _dbContext.PartyGroups.Include(g => g.Members).AsNoTracking();
            IQueryable<SpatialUnit> spatialQuery = _dbContext.SpatialUnits.AsNoTracking();

            if (AsOfDate.HasValue)
            {
                var date = AsOfDate.Value;
                baQuery = _dbContext.BAUnits.TemporalAsOf(date).AsNoTracking();
                partyQuery = _dbContext.Parties.TemporalAsOf(date).AsNoTracking();
                rrrQuery = _dbContext.Rrrs.TemporalAsOf(date).AsNoTracking();
                sourceQuery = _dbContext.Sources.TemporalAsOf(date).AsNoTracking();
                groupQuery = _dbContext.PartyGroups.TemporalAsOf(date).Include(g => g.Members).AsNoTracking();
                spatialQuery = _dbContext.SpatialUnits.TemporalAsOf(date).AsNoTracking();
                StatusMessage = $"РЕЖИМ ИСТОРИЧЕСКОГО СРЕЗА: Вы просматриваете состояние реестра на {date:dd.MM.yyyy HH:mm} (UTC). Редактирование записей заблокировано.";
            }

            if (!string.IsNullOrEmpty(FilterType))
            {
                if (Tab == "spatial" && Enum.TryParse<SpatialUnitType>(FilterType, out var suType)) spatialQuery = spatialQuery.Where(x => x.Type == suType);
                if (Tab == "baunits" && Enum.TryParse<BAUnitType>(FilterType, out var baType)) baQuery = baQuery.Where(x => x.Type == baType);
                if (Tab == "parties" && Enum.TryParse<PartyType>(FilterType, out var pType)) partyQuery = partyQuery.Where(x => x.Type == pType);
                if (Tab == "rrr" && Enum.TryParse<RRRType>(FilterType, out var rType)) rrrQuery = rrrQuery.Where(x => x.Type == rType);
                if (Tab == "sources" && Enum.TryParse<SourceType>(FilterType, out var sType)) sourceQuery = sourceQuery.Where(x => x.Type == sType);
            }

            if (FilterDateFrom.HasValue)
            {
                var from = DateTime.SpecifyKind(FilterDateFrom.Value, DateTimeKind.Utc);
                if (Tab == "rrr") rrrQuery = rrrQuery.Where(x => x.StartDate >= from);
                if (Tab == "sources") sourceQuery = sourceQuery.Where(x => x.RecordDate >= from);
            }

            if (FilterDateTo.HasValue)
            {
                var to = DateTime.SpecifyKind(FilterDateTo.Value, DateTimeKind.Utc);
                if (Tab == "rrr") rrrQuery = rrrQuery.Where(x => x.StartDate <= to);
                if (Tab == "sources") sourceQuery = sourceQuery.Where(x => x.RecordDate <= to);
            }

            if (Tab == "baunits")
            {
                TotalItems = await baQuery.CountAsync();
                var baUnitsList = await baQuery.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(PageSize).ToListAsync();

                var allSpatialIds = baUnitsList.SelectMany(b => b.SpatialUnits.Select(su => su.SpatialUnitId)).Distinct().ToList();
                var spatialRefs = await _dbContext.SpatialUnits.Where(s => allSpatialIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.ReferenceNumber);

                BAUnits = baUnitsList.Select(b => new BAUnitViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    TypeStr = Translate(b.Type),
                    IsDeleted = b.IsDeleted,
                    SpatialUnitIdsMap = b.SpatialUnits.ToDictionary(su => su.SpatialUnitId, su => spatialRefs.ContainsKey(su.SpatialUnitId) ? spatialRefs[su.SpatialUnitId] : "КН не найден")
                }).ToList();
            }
            else if (Tab == "parties")
            {
                TotalItems = await partyQuery.CountAsync();
                Parties = await partyQuery.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(PageSize)
                    .Select(p => new PartyViewModel
                    {
                        Id = p.Id,
                        ExtId = p.ExtId,
                        Name = p.Name,
                        TypeStr = Translate(p.Type),
                        ContactInfo = p.ContactInfo,
                        IsDeleted = p.IsDeleted
                    })
                    .ToListAsync();
            }
            else if (Tab == "rrr")
            {
                TotalItems = await rrrQuery.CountAsync();
                var rrrList = await rrrQuery.OrderByDescending(x => x.StartDate).Skip(skip).Take(PageSize).ToListAsync();

                var baIds = rrrList.Select(r => r.BAUnitId).Distinct().ToList();
                var srcIds = rrrList.Select(r => r.SourceId).Distinct().ToList();
                var partyIds = rrrList.Where(r => r.PartyId.HasValue).Select(r => r.PartyId.Value).Distinct().ToList();
                var groupIds = rrrList.Where(r => r.PartyGroupId.HasValue).Select(r => r.PartyGroupId.Value).Distinct().ToList();

                var baDict = await _dbContext.BAUnits.Where(b => baIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.Name);
                var srcDict = await _dbContext.Sources.Where(s => srcIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => $"№ {s.DocumentNumber} от {s.RecordDate:dd.MM.yyyy}");
                var partyDict = await _dbContext.Parties.Where(p => partyIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name);
                var groupDict = await _dbContext.PartyGroups.Where(g => groupIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name);

                Rrrs = rrrList.Select(r => new RrrViewModel
                {
                    Id = r.Id,
                    TypeStr = Translate(r.Type),
                    BAUnitId = r.BAUnitId,
                    BAUnitName = baDict.ContainsKey(r.BAUnitId) ? baDict[r.BAUnitId] : "Связь утеряна",
                    SourceName = srcDict.ContainsKey(r.SourceId) ? srcDict[r.SourceId] : "Документ не найден",
                    SubjectName = r.PartyId.HasValue && partyDict.ContainsKey(r.PartyId.Value) ? partyDict[r.PartyId.Value] :
                                  (r.PartyGroupId.HasValue && groupDict.ContainsKey(r.PartyGroupId.Value) ? $"Группа: {groupDict[r.PartyGroupId.Value]}" : "Субъект не найден"),
                    ShareNumerator = r.ShareNumerator,
                    ShareDenominator = r.ShareDenominator,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    IsDeleted = r.IsDeleted
                }).ToList();
            }
            else if (Tab == "sources")
            {
                TotalItems = await sourceQuery.CountAsync();
                Sources = await sourceQuery.OrderByDescending(x => x.RecordDate).Skip(skip).Take(PageSize)
                    .Select(s => new SourceViewModel
                    {
                        Id = s.Id,
                        TypeStr = Translate(s.Type),
                        DocumentNumber = s.DocumentNumber,
                        RecordDate = s.RecordDate,
                        ContentUrl = s.ContentUrl,
                        IsDeleted = s.IsDeleted
                    })
                    .ToListAsync();
            }
            else if (Tab == "partygroups")
            {
                TotalItems = await groupQuery.CountAsync();
                var groupsList = await groupQuery.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(PageSize).ToListAsync();

                var partyIds = groupsList.SelectMany(g => g.Members).Select(m => m.PartyId).Distinct().ToList();
                var partyDict = await _dbContext.Parties.Where(p => partyIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name);

                PartyGroups = groupsList.Select(g => new PartyGroupViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsDeleted = g.IsDeleted,
                    Members = g.Members.Select(m => new PartyGroupMemberViewModel
                    {
                        PartyId = m.PartyId,
                        PartyName = partyDict.ContainsKey(m.PartyId) ? partyDict[m.PartyId] : "Неизвестный участник",
                        ShareNumerator = m.ShareNumerator,
                        ShareDenominator = m.ShareDenominator
                    }).ToList()
                }).ToList();
            }
            else if (Tab == "spatial")
            {
                TotalItems = await spatialQuery.CountAsync();
                SpatialUnits = await spatialQuery.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(PageSize)
                    .Select(su => new SpatialUnitViewModel
                    {
                        Id = su.Id,
                        ReferenceNumber = su.ReferenceNumber,
                        TypeStr = Translate(su.Type),
                        AreaSqMeters = su.AreaSqMeters,
                        IsDeleted = su.IsDeleted
                    })
                    .ToListAsync();
            }
        }

        private static string Translate(Enum value) => value switch
        {
            SpatialUnitType.Parcel => "Земельный участок",
            SpatialUnitType.Building => "Здание",
            SpatialUnitType.Room => "Помещение",
            SpatialUnitType.Volume3D => "Пространственный объем",
            BAUnitType.BasicPropertyUnit => "Базовый объект недвижимости",
            BAUnitType.LeasedUnit => "Объект аренды",
            BAUnitType.RightOfUseUnit => "Объект пользования",
            PartyType.NaturalPerson => "Физическое лицо",
            PartyType.NonNaturalPerson => "Юридическое лицо",
            PartyType.Municipality => "Муниципальное образование",
            PartyType.State => "Государственный субъект",
            RRRType.Ownership => "Право собственности",
            RRRType.Lease => "Право аренды",
            RRRType.Mortgage => "Залоговое обременение",
            RRRType.Servitude => "Ограничение (Сервитут)",
            RRRType.Usufruct => "Право узуфрукта",
            SourceType.SaleContract => "Договор купли-продажи",
            SourceType.CourtDecision => "Судебное постановление",
            SourceType.InheritanceCertificate => "Свидетельство о наследстве",
            SourceType.AdministrativeAct => "Государственный акт",
            _ => value.ToString()
        };

        [Authorize(Policy = Permissions.DataDelete)]
        public async Task<IActionResult> OnPostDeleteDataAsync(Guid id, string entityType)
        {
            try
            {
                if (entityType == "BAUnit") { var e = await _dbContext.BAUnits.FindAsync(id); if (e != null) _dbContext.BAUnits.Remove(e); }
                else if (entityType == "SpatialUnit") { var e = await _dbContext.SpatialUnits.FindAsync(id); if (e != null) _dbContext.SpatialUnits.Remove(e); }
                else if (entityType == "Party") { var e = await _dbContext.Parties.FindAsync(id); if (e != null) _dbContext.Parties.Remove(e); }
                else if (entityType == "RRR") { var e = await _dbContext.Rrrs.FindAsync(id); if (e != null) _dbContext.Rrrs.Remove(e); }
                else if (entityType == "Source") { var e = await _dbContext.Sources.FindAsync(id); if (e != null) _dbContext.Sources.Remove(e); }

                await _dbContext.SaveChangesAsync();
                StatusMessage = "Запись успешно перемещена в архивный фонд.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Критическая ошибка при архивировании: {ex.Message}";
            }
            return RedirectToPage(new { tab = Request.Form["Tab"] });
        }

        private string GetFullErrorMessage(Exception ex)
        {
            var msg = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                msg += " | Системные детали: " + inner.Message;
                inner = inner.InnerException;
            }
            return msg;
        }

        public async Task<IActionResult> OnPostCreateBAUnitAsync()
        {
            try { await _mediator.Send(BAUnitInput); StatusMessage = "Объект недвижимости успешно зарегистрирован."; }
            catch (Exception ex) { StatusMessage = $"Ошибка регистрации: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "baunits" });
        }

        public async Task<IActionResult> OnPostCreatePartyAsync()
        {
            try { await _mediator.Send(PartyInput); StatusMessage = "Данные правообладателя успешно внесены в систему."; }
            catch (Exception ex) { StatusMessage = $"Ошибка внесения: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "parties" });
        }

        public async Task<IActionResult> OnPostUpdatePartyContactAsync(Guid id, string contactInfo)
        {
            try { await _mediator.Send(new UpdatePartyContactInfoCommand(id, contactInfo)); StatusMessage = "Контактная информация правообладателя успешно обновлена."; }
            catch (Exception ex) { StatusMessage = $"Ошибка обновления: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "parties" });
        }

        public async Task<IActionResult> OnPostCreateRRRAsync()
        {
            try
            {
                Guid? partyId = RRRSubjectType == "Individual" ? RRRInput.PartyId : null;
                Guid? groupId = RRRSubjectType == "Group" ? RRRInput.PartyGroupId : null;

                var cmd = new RegisterRRRCommand(
                    RRRInput.Type,
                    RRRInput.BAUnitId,
                    partyId,
                    groupId,
                    RRRInput.SourceId,
                    RRRInput.ShareNumerator,
                    RRRInput.ShareDenominator,
                    DateTime.SpecifyKind(RRRInput.StartDate, DateTimeKind.Utc));

                await _mediator.Send(cmd);
                StatusMessage = "Право успешно зарегистрировано в едином реестре.";
            }
            catch (Exception ex) { StatusMessage = $"Ошибка регистрации права: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "rrr" });
        }

        public async Task<IActionResult> OnPostTerminateRRRAsync()
        {
            try { await _mediator.Send(TerminateRRRInput with { EndDate = DateTime.SpecifyKind(TerminateRRRInput.EndDate, DateTimeKind.Utc) }); StatusMessage = "Указанное право успешно погашено."; }
            catch (Exception ex) { StatusMessage = $"Ошибка погашения: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "rrr" });
        }

        public async Task<IActionResult> OnPostCreateSourceAsync()
        {
            try
            {
                string localUrl = string.Empty;
                if (UploadedFile != null)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "sources");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadedFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) await UploadedFile.CopyToAsync(fileStream);
                    localUrl = "/uploads/sources/" + fileName;
                }

                await _mediator.Send(new CreateSourceCommand(SourceInput.Type, SourceInput.DocumentNumber, DateTime.SpecifyKind(SourceInput.RecordDate, DateTimeKind.Utc), localUrl));
                StatusMessage = "Документ-основание загружен в архивный фонд.";
            }
            catch (Exception ex) { StatusMessage = $"Ошибка сохранения документа: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "sources" });
        }

        public async Task<IActionResult> OnPostCreatePartyGroupAsync()
        {
            try { await _mediator.Send(PartyGroupInput); StatusMessage = "Группа совместной собственности успешно создана."; }
            catch (Exception ex) { StatusMessage = $"Ошибка создания группы: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "partygroups" });
        }

        public async Task<IActionResult> OnPostAddPartyToGroupAsync()
        {
            try { await _mediator.Send(AddPartyToGroupInput); StatusMessage = "Правообладатель успешно добавлен в группу."; }
            catch (Exception ex) { StatusMessage = $"Ошибка добавления участника: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "partygroups" });
        }

        public async Task<IActionResult> OnPostRemovePartyFromGroupAsync(Guid groupId, Guid partyId)
        {
            try { await _mediator.Send(new RemovePartyFromGroupCommand(groupId, partyId)); StatusMessage = "Правообладатель успешно исключен из состава группы."; }
            catch (Exception ex) { StatusMessage = $"Ошибка исключения участника: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "partygroups" });
        }

        public async Task<IActionResult> OnPostAddSpatialUnitToBAUnitAsync()
        {
            try { await _mediator.Send(AddSpatialUnitInput); StatusMessage = "Пространственный контур успешно привязан к объекту недвижимости."; }
            catch (Exception ex) { StatusMessage = $"Ошибка связывания данных: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "baunits" });
        }

        public async Task<IActionResult> OnPostRemoveSpatialUnitFromBAUnitAsync(Guid baUnitId, Guid spatialUnitId)
        {
            try { await _mediator.Send(new RemoveSpatialUnitFromBAUnitCommand(baUnitId, spatialUnitId)); StatusMessage = "Связь пространственного контура с объектом недвижимости разорвана."; }
            catch (Exception ex) { StatusMessage = $"Ошибка удаления связи: {GetFullErrorMessage(ex)}"; }
            return RedirectToPage(new { tab = "baunits" });
        }
    }
}