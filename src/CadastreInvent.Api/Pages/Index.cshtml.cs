using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Api.Services;
using CadastreInvent.Shared.Application.Auth;

namespace CadastreInvent.Api.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(CadastreDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        public int TotalBAUnits { get; set; }
        public int TotalSpatialUnits { get; set; }
        public int TotalParties { get; set; }
        public int TotalRrrs { get; set; }

        public List<ActivityRecord> RecentActivities { get; set; } = new();

        public bool IsAdministrator { get; set; }
        public ThemeSettings CurrentSettings { get; set; }

        [BindProperty] public ThemeSettings SettingsInput { get; set; }
        [BindProperty] public IFormFile LogoUpload { get; set; }
        [TempData] public string StatusMessage { get; set; }

        public class ActivityRecord
        {
            public Guid Id { get; set; }
            public string ActionType { get; set; }
            public string EntityType { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public async Task OnGetAsync()
        {
            IsAdministrator = User.IsInRole(AppRoles.Admin);
            CurrentSettings = ThemeSettingsManager.GetSettings();
            SettingsInput = CurrentSettings;

            TotalBAUnits = await _dbContext.BAUnits.CountAsync(x => !EF.Property<bool>(x, "IsDeleted"));
            TotalSpatialUnits = await _dbContext.SpatialUnits.CountAsync(x => !EF.Property<bool>(x, "IsDeleted"));
            TotalParties = await _dbContext.Parties.CountAsync(x => !EF.Property<bool>(x, "IsDeleted"));
            TotalRrrs = await _dbContext.Rrrs.CountAsync(x => !EF.Property<bool>(x, "IsDeleted"));

            var rawLogs = await _dbContext.AuditLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(10)
                .Select(x => new
                {
                    x.EntityId,
                    x.Action,
                    x.EntityName,
                    x.Timestamp
                })
                .ToListAsync();

            RecentActivities = rawLogs.Select(x => new ActivityRecord
            {
                Id = x.EntityId,
                ActionType = TranslateAction(x.Action),
                EntityType = TranslateEntity(x.EntityName),
                Timestamp = x.Timestamp
            }).ToList();
        }

        public async Task<IActionResult> OnPostSaveSettingsAsync()
        {
            if (!User.IsInRole(AppRoles.Admin))
            {
                return Forbid();
            }

            var settings = ThemeSettingsManager.GetSettings();
            settings.CompanyName = SettingsInput.CompanyName;
            settings.SystemName = SettingsInput.SystemName;
            settings.Description = SettingsInput.Description;

            if (LogoUpload != null && LogoUpload.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "branding");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = "company-logo" + Path.GetExtension(LogoUpload.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await LogoUpload.CopyToAsync(fileStream);
                }
                settings.LogoUrl = "/uploads/branding/" + fileName + "?v=" + DateTime.UtcNow.Ticks;
            }

            ThemeSettingsManager.SaveSettings(settings);
            StatusMessage = "Системные параметры платформы успешно обновлены.";

            return RedirectToPage();
        }

        private static string TranslateAction(string action) => action switch
        {
            "Added" => "Регистрация записи",
            "Modified" => "Модификация данных",
            "Deleted" => "Архивирование",
            _ => action
        };

        private static string TranslateEntity(string entity) => entity switch
        {
            "SpatialUnit" => "Пространственный контур",
            "BAUnit" => "Имущественный объект",
            "Party" => "Правообладатель",
            "PartyGroup" => "Совместное объединение",
            "RRR" => "Правоотношение",
            "Source" => "Документальная база",
            _ => entity
        };
    }
}