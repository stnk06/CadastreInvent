using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Api.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<CadastreDbContext>>();

        try
        {
            var context = services.GetRequiredService<CadastreDbContext>();

            logger.LogInformation("Начало применения миграций базы данных...");

            await context.Database.MigrateAsync();

            logger.LogInformation("Миграции базы данных успешно применены.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Произошла критическая ошибка при применении миграций к базе данных.");
            throw;
        }
    }
}