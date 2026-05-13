using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Auth;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Shared.Application.Interfaces;
using CadastreInvent.Valuation.Domain.Entities;

namespace CadastreInvent.Api.Pages
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ArchiveModel : PageModel
    {
        private readonly CadastreDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public ArchiveModel(CadastreDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "spatial";

        [TempData]
        public string StatusMessage { get; set; }

        public class ArchiveItemDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public DateTime? DeletedAt { get; set; }
            public string EntityType { get; set; }
        }

        public List<ArchiveItemDto> DeletedItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(Tab)) Tab = "spatial";

            if (Tab == "spatial")
            {
                DeletedItems = await _dbContext.SpatialUnits.IgnoreQueryFilters()
                    .Where(x => EF.Property<bool>(x, "IsDeleted"))
                    .Select(x => new ArchiveItemDto { Id = x.Id, Title = x.ReferenceNumber, Subtitle = "Тип: " + x.Type, DeletedAt = EF.Property<DateTime?>(x, "DeletedAt"), EntityType = "SpatialUnit" })
                    .ToListAsync();
            }
            else if (Tab == "baunits")
            {
                DeletedItems = await _dbContext.BAUnits.IgnoreQueryFilters()
                    .Where(x => EF.Property<bool>(x, "IsDeleted"))
                    .Select(x => new ArchiveItemDto { Id = x.Id, Title = x.Name, Subtitle = "Категория: " + x.Type, DeletedAt = EF.Property<DateTime?>(x, "DeletedAt"), EntityType = "BAUnit" })
                    .ToListAsync();
            }
            else if (Tab == "parties")
            {
                DeletedItems = await _dbContext.Parties.IgnoreQueryFilters()
                    .Where(x => EF.Property<bool>(x, "IsDeleted"))
                    .Select(x => new ArchiveItemDto { Id = x.Id, Title = x.Name, Subtitle = "ИНН/БИН: " + x.ExtId, DeletedAt = EF.Property<DateTime?>(x, "DeletedAt"), EntityType = "Party" })
                    .ToListAsync();
            }
            else if (Tab == "rrr")
            {
                DeletedItems = await _dbContext.Rrrs.IgnoreQueryFilters()
                    .Where(x => EF.Property<bool>(x, "IsDeleted"))
                    .Select(x => new ArchiveItemDto { Id = x.Id, Title = "Право: " + x.Type, Subtitle = "Доля: " + x.ShareNumerator + "/" + x.ShareDenominator, DeletedAt = EF.Property<DateTime?>(x, "DeletedAt"), EntityType = "RRR" })
                    .ToListAsync();
            }
            else if (Tab == "sources")
            {
                DeletedItems = await _dbContext.Sources.IgnoreQueryFilters()
                    .Where(x => EF.Property<bool>(x, "IsDeleted"))
                    .Select(x => new ArchiveItemDto { Id = x.Id, Title = "Документ № " + x.DocumentNumber, Subtitle = "Вид: " + x.Type, DeletedAt = EF.Property<DateTime?>(x, "DeletedAt"), EntityType = "Source" })
                    .ToListAsync();
            }
            else if (Tab == "models")
            {
                DeletedItems = await _dbContext.MassAppraisalModels.IgnoreQueryFilters()
                    .Where(x => EF.Property<bool>(x, "IsDeleted"))
                    .Select(x => new ArchiveItemDto { Id = x.Id, Title = "Модель: " + x.Version, Subtitle = "Алгоритм: " + x.Algorithm + " | " + x.Description, DeletedAt = EF.Property<DateTime?>(x, "DeletedAt"), EntityType = "MassAppraisalModel" })
                    .ToListAsync();
            }

            DeletedItems = DeletedItems.OrderByDescending(x => x.DeletedAt).ToList();
        }

        public async Task<IActionResult> OnPostRestoreAsync(Guid id, string entityType)
        {
            try
            {
                object entity = null;
                if (entityType == "SpatialUnit") entity = await _dbContext.SpatialUnits.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
                else if (entityType == "BAUnit") entity = await _dbContext.BAUnits.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
                else if (entityType == "Party") entity = await _dbContext.Parties.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
                else if (entityType == "RRR") entity = await _dbContext.Rrrs.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
                else if (entityType == "Source") entity = await _dbContext.Sources.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
                else if (entityType == "MassAppraisalModel") entity = await _dbContext.MassAppraisalModels.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);

                if (entity != null)
                {
                    var entry = _dbContext.Entry(entity);
                    entry.Property("IsDeleted").CurrentValue = false;
                    entry.Property("DeletedAt").CurrentValue = null;
                    entry.State = EntityState.Modified;

                    await _dbContext.SaveChangesAsync();
                    StatusMessage = "Успех! Запись успешно восстановлена.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при восстановлении: {ex.Message}";
            }
            return RedirectToPage(new { tab = Request.Form["Tab"] });
        }

        public async Task<IActionResult> OnPostHardDeleteAsync(Guid id, string entityType)
        {
            try
            {
                string schema = "registry";
                string table = "";

                if (entityType == "SpatialUnit") table = "spatial_units";
                else if (entityType == "BAUnit") table = "ba_units";
                else if (entityType == "Party") table = "parties";
                else if (entityType == "RRR") table = "rrrs";
                else if (entityType == "Source") table = "sources";
                else if (entityType == "MassAppraisalModel")
                {
                    schema = "valuation";
                    table = "mass_appraisal_models";
                    await _dbContext.Database.ExecuteSqlRawAsync($"UPDATE valuation.valuations SET \"ModelId\" = NULL WHERE \"ModelId\" = '{id}'");
                }

                if (!string.IsNullOrEmpty(table))
                {
                    await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {schema}.{table} WHERE \"Id\" = '{id}'");

                    var userId = _currentUserService?.UserId ?? Guid.Empty;
                    var audit = new AuditLog(id, entityType, "HardDeleted", "{\"Action\": \"Физическое уничтожение записи Администратором\"}", userId);
                    _dbContext.AuditLogs.Add(audit);
                    await _dbContext.SaveChangesAsync();

                    StatusMessage = "Внимание! Запись была безвозвратно уничтожена из базы данных.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при жестком удалении: {ex.Message}";
            }
            return RedirectToPage(new { tab = Request.Form["Tab"] });
        }
    }
}