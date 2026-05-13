using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.Commands;

namespace CadastreInvent.Api.Pages
{
    [Authorize]
    public class ValuationModel : PageModel
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IMediator _mediator;

        public ValuationModel(CadastreDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "dashboard";
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public string FilterType { get; set; }

        [TempData] public string StatusMessage { get; set; }

        public class BAUnitLookup { public Guid Id { get; set; } public string Name { get; set; } }
        public List<BAUnitLookup> AvailableBAUnits { get; set; } = new();

        [BindProperty] public CreateValuationUnitCommand CreateUnitInput { get; set; }
        [BindProperty] public UpdateValuationUnitZoningCommand UpdateZoningInput { get; set; }
        [BindProperty] public AddPropertyCharacteristicCommand AddCharacteristicInput { get; set; }
        [BindProperty] public UpdatePropertyCharacteristicsCommand UpdateCharacteristicInput { get; set; }
        [BindProperty] public RegisterSalesTransactionCommand RegisterSaleInput { get; set; }
        [BindProperty] public InvalidateSalesTransactionCommand InvalidateSaleInput { get; set; }
        [BindProperty] public CreateValuationAppealCommand CreateAppealInput { get; set; }
        [BindProperty] public UpdateValuationAppealStatusCommand UpdateAppealInput { get; set; }
        [BindProperty] public ExecuteMassAppraisalCommand ExecuteAppraisalInput { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(Tab)) Tab = "dashboard";
            if (PageNumber < 1) PageNumber = 1;

            AvailableBAUnits = await _dbContext.BAUnits
                .AsNoTracking()
                .Where(b => !EF.Property<bool>(b, "IsDeleted"))
                .Select(b => new BAUnitLookup { Id = b.Id, Name = b.Name })
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostTrainAsync()
        {
            try
            {
                await _mediator.Send(new TrainMassAppraisalModelCommand($"v{DateTime.UtcNow:yyyyMMdd-HHmm}", "Ручной запуск обучения модели"));
                StatusMessage = "Процесс обучения алгоритма запущен в изолированном фоне.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка запуска: {ex.Message}";
            }
            return RedirectToPage(new { tab = "diagnostics" });
        }

        public async Task<IActionResult> OnPostCreateValuationUnitAsync()
        {
            try
            {
                await _mediator.Send(CreateUnitInput);
                StatusMessage = "Объект успешно поставлен на оценку.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка регистрации объекта: {ex.Message}";
            }
            return RedirectToPage(new { tab = "units" });
        }

        public async Task<IActionResult> OnPostUpdateZoningAsync()
        {
            try
            {
                await _mediator.Send(UpdateZoningInput);
                StatusMessage = "Территориальное зонирование обновлено.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления зоны: {ex.Message}";
            }
            return RedirectToPage(new { tab = "units" });
        }

        public async Task<IActionResult> OnPostAddCharacteristicAsync()
        {
            try
            {
                await _mediator.Send(AddCharacteristicInput);
                StatusMessage = "Матрица характеристик сохранена.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка сохранения характеристик: {ex.Message}";
            }
            return RedirectToPage(new { tab = "units" });
        }

        public async Task<IActionResult> OnPostUpdateCharacteristicAsync()
        {
            try
            {
                await _mediator.Send(UpdateCharacteristicInput);
                StatusMessage = "Матрица характеристик обновлена.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка обновления характеристик: {ex.Message}";
            }
            return RedirectToPage(new { tab = "units" });
        }

        public async Task<IActionResult> OnPostRegisterSaleAsync()
        {
            try
            {
                await _mediator.Send(RegisterSaleInput);
                StatusMessage = "Рыночная сделка успешно внесена в реестр цен.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка регистрации транзакции: {ex.Message}";
            }
            return RedirectToPage(new { tab = "sales" });
        }

        public async Task<IActionResult> OnPostInvalidateSaleAsync()
        {
            try
            {
                await _mediator.Send(InvalidateSaleInput);
                StatusMessage = "Сделка успешно выбракована и исключена из алгоритмов ML.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка выбраковки: {ex.Message}";
            }
            return RedirectToPage(new { tab = "sales" });
        }

        public async Task<IActionResult> OnPostCreateAppealAsync()
        {
            try
            {
                await _mediator.Send(CreateAppealInput);
                StatusMessage = "Апелляционное производство зарегистрировано.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка регистрации апелляции: {ex.Message}";
            }
            return RedirectToPage(new { tab = "appeals" });
        }

        public async Task<IActionResult> OnPostUpdateAppealAsync()
        {
            try
            {
                await _mediator.Send(UpdateAppealInput);
                StatusMessage = "Статус рассмотрения апелляции изменен.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка изменения статуса: {ex.Message}";
            }
            return RedirectToPage(new { tab = "appeals" });
        }

        public async Task<IActionResult> OnPostExecuteAppraisalAsync()
        {
            try
            {
                await _mediator.Send(ExecuteAppraisalInput);
                StatusMessage = "Процесс массовой оценки инициирован.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка запуска оценки: {ex.Message}";
            }
            return RedirectToPage(new { tab = "diagnostics" });
        }
    }
}