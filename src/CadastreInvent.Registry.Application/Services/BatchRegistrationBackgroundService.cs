using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Infrastructure.Integration;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Registry.Application.Services
{
    public class BatchRegistrationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BatchRegistrationBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessPendingItemsAsync(stoppingToken);
            }
        }

        private async Task ProcessPendingItemsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CadastreDbContext>();
            var externalService = scope.ServiceProvider.GetRequiredService<IExternalCadastreService>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var transformService = scope.ServiceProvider.GetRequiredService<ICoordinateTransformationService>();

            var executionStrategy = dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
                try
                {
                    var pendingItems = await dbContext.BatchRegistrationItems
                        .FromSqlRaw("SELECT * FROM registry.batch_registration_items WHERE \"Status\" = 'Pending' FOR UPDATE SKIP LOCKED LIMIT 5")
                        .ToListAsync(stoppingToken);

                    if (!pendingItems.Any())
                    {
                        await transaction.RollbackAsync(stoppingToken);
                        return;
                    }

                    var random = new Random();

                    foreach (var item in pendingItems)
                    {
                        try
                        {
                            var reader = new WKTReader();
                            var geometry = reader.Read(item.Wkt);

                            if (geometry is Polygon polygon)
                            {
                                polygon.SRID = 4326;

                                var metricPolygon = transformService.TransformFromWgs84(polygon, 28408);
                                double area = metricPolygon.Area;

                                string cadNumber = $"77:01:{random.Next(1000000, 9999999)}:{random.Next(1000, 9999)}";

                                await transaction.CreateSavepointAsync("BeforeProperty", stoppingToken);
                                try
                                {
                                    var spatialUnitId = await mediator.Send(new CreateSpatialUnitCommand(
                                        cadNumber,
                                        SpatialUnitType.Parcel,
                                        item.Wkt,
                                        area), stoppingToken);

                                    string address = $"Объект недвижимости (Кад. № {cadNumber})";
                                    try
                                    {
                                        var externalDataList = await externalService.GetPropertiesInAreaAsync(item.Wkt, stoppingToken);
                                        if (externalDataList.Any() && !string.IsNullOrWhiteSpace(externalDataList.First().Address))
                                        {
                                            address = externalDataList.First().Address;
                                        }
                                    }
                                    catch { }

                                    var baUnitId = await mediator.Send(new CreateBAUnitCommand(
                                        address,
                                        BAUnitType.BasicPropertyUnit), stoppingToken);

                                    await mediator.Send(new AddSpatialUnitToBAUnitCommand(baUnitId, spatialUnitId), stoppingToken);

                                    item.MarkProcessed(cadNumber);
                                }
                                catch (Exception)
                                {
                                    await transaction.RollbackToSavepointAsync("BeforeProperty", stoppingToken);
                                    throw;
                                }
                            }
                            else
                            {
                                item.MarkFailed("Ошибка: Переданная геометрия не является полигоном.");
                            }
                        }
                        catch (Exception ex)
                        {
                            item.MarkFailed(ex.Message);
                        }

                        var job = await dbContext.BatchRegistrationJobs.FirstOrDefaultAsync(j => j.Id == item.JobId, stoppingToken);
                        if (job != null)
                        {
                            job.IncrementProcessed();
                            if (job.ProcessedCount >= job.TotalCount)
                            {
                                job.Complete();
                            }
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(stoppingToken);
                }
            });
        }
    }
}