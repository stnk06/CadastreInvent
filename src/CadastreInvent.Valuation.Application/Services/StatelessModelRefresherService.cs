using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.ML;

namespace CadastreInvent.Valuation.Application.Services
{
    public class StatelessModelRefresherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StatelessModelRefresherService> _logger;
        private Guid _lastLoadedModelId = Guid.Empty;

        public StatelessModelRefresherService(
            IServiceProvider serviceProvider,
            ILogger<StatelessModelRefresherService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CheckAndLoadModelAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                await CheckAndLoadModelAsync(stoppingToken);
            }
        }

        private async Task CheckAndLoadModelAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CadastreDbContext>();
                var mlService = scope.ServiceProvider.GetRequiredService<IMassAppraisalMLService>();

                var latestModelId = await dbContext.MassAppraisalModels
                    .AsNoTracking()
                    .Where(m => m.Status == "Active" && !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Id)
                    .FirstOrDefaultAsync(stoppingToken);

                if (latestModelId != Guid.Empty && latestModelId != _lastLoadedModelId)
                {
                    var fullModel = await dbContext.MassAppraisalModels
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == latestModelId, stoppingToken);

                    if (fullModel != null && fullModel.ModelData != null)
                    {
                        _logger.LogInformation("Обнаружена новая ML-модель {Version}. Загрузка в пул оперативной памяти...", fullModel.Version);

                        mlService.LoadModelFromBytes(fullModel.ModelData, fullModel.Version);

                        _lastLoadedModelId = latestModelId;
                        _logger.LogInformation("Модель {Version} успешно загружена и готова к расчетам.", fullModel.Version);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при загрузке ML-модели из БД в оперативную память.");
            }
        }
    }
}