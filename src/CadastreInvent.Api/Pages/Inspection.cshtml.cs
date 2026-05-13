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
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Inspection.Application.Commands;
using CadastreInvent.Inspection.Application.Queries;
using CadastreInvent.Shared.Application.Auth;

namespace CadastreInvent.Api.Pages
{
    [Authorize]
    public class InspectionModel : PageModel
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IMediator _mediator;

        public InspectionModel(CadastreDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public bool IsDispatcher { get; set; }
        public string DispatcherDataJson { get; set; } = "[]";
        public Dictionary<Guid, string> AvailableInspectors { get; set; } = new();

        [TempData] public string StatusMessage { get; set; }

        [BindProperty] public CreateTaskDto CreateTaskInput { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IsDispatcher = User.IsInRole(AppRoles.Admin);

            if (!IsDispatcher)
            {
                return Page();
            }

            var tasks = await _mediator.Send(new GetDispatcherTasksQuery());

            AvailableInspectors = await _dbContext.Users.AsNoTracking()
                .Where(u => u.IsActive)
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            DispatcherDataJson = JsonSerializer.Serialize(tasks, options);

            return Page();
        }

        public async Task<IActionResult> OnPostCreateTaskAsync()
        {
            if (!User.IsInRole(AppRoles.Admin)) return Forbid();

            try
            {
                var cmd = new CreateInspectionTaskCommand(
                    CreateTaskInput.SpatialUnitId,
                    CreateTaskInput.InspectorId,
                    DateTime.SpecifyKind(CreateTaskInput.Deadline, DateTimeKind.Utc));

                await _mediator.Send(cmd);
                StatusMessage = "Ордер успешно сформирован. Координаты вычислены автоматически.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Сбой системы: {ex.Message}";
            }

            return RedirectToPage();
        }

        public class CreateTaskDto
        {
            public Guid SpatialUnitId { get; set; }
            public Guid InspectorId { get; set; }
            public DateTime Deadline { get; set; }
        }
    }
}