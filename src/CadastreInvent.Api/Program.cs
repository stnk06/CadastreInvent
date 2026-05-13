using CadastreInvent.Api.Auth;
using CadastreInvent.Api.Behaviors;
using CadastreInvent.Api.Extensions;
using CadastreInvent.Api.Hubs;
using CadastreInvent.Api.Middlewares;
using CadastreInvent.Api.Services;
using CadastreInvent.Infrastructure.Auth;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Infrastructure.Reports;
using CadastreInvent.Infrastructure.Services;
using CadastreInvent.Infrastructure.Services.Excel;
using CadastreInvent.Inspection.Application.Commands;
using CadastreInvent.Inspection.Application.Validators;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Registry.Application.Services;
using CadastreInvent.Registry.Application.Validators;
using CadastreInvent.Shared.Application.Auth;
using CadastreInvent.Shared.Application.Interfaces;
using CadastreInvent.Shared.Application.Reports;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Application.ML;
using CadastreInvent.Valuation.Application.Services;
using CadastreInvent.Valuation.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Host=127.0.0.1;Port=5432;Database=cadastre_db;Username=postgres;Password=SuperSecretPassword123!";

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Storage"));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();
builder.Services.AddScoped<IInspectionNotificationService, InspectionNotificationService>();
builder.Services.AddScoped<ISpatialValidationService, SpatialValidationService>();
builder.Services.AddSingleton<ICoordinateTransformationService, CoordinateTransformationService>();
builder.Services.AddSingleton<IMassAppraisalNotificationService, MassAppraisalNotificationService>();

builder.Services.AddScoped<IExcelImportService, ExcelImportService>();

QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IDocumentGeneratorService, DocumentGeneratorService>();

builder.Services.AddHttpClient<CadastreInvent.Infrastructure.Integration.IExternalCadastreService, CadastreInvent.Infrastructure.Integration.DadataApiClient>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddSingleton<IMassAppraisalDiagnosticLogger, MassAppraisalDiagnosticLogger>();

builder.Services.AddSingleton<IMassAppraisalMLService, MassAppraisalMLService>();
builder.Services.AddScoped<ISpatialFeatureService, SpatialFeatureService>();

builder.Services.AddHostedService<StatelessModelRefresherService>();

builder.Services.AddSingleton<IMlTrainingQueue, MlTrainingQueue>();
builder.Services.AddHostedService<MlTrainingBackgroundService>();

builder.Services.AddSingleton<IMassAppraisalQueue, MassAppraisalQueue>();
builder.Services.AddHostedService<MassAppraisalBackgroundService>();

// --- REGISTRY SERVICES ---
builder.Services.AddHostedService<BatchRegistrationBackgroundService>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddDbContext<CadastreDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseNetTopologySuite();
        npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
    });
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateSpatialUnitCommand>();
    cfg.RegisterServicesFromAssemblyContaining<AddPropertyCharacteristicCommand>();
    cfg.RegisterServicesFromAssemblyContaining<StartInspectionTaskCommand>();
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateSpatialUnitCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddPropertyCharacteristicCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddInspectionObservationCommandValidator>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key ?? "CadastreInventEnterpriseSuperSecretKey2026!@#Secure"))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                return Task.CompletedTask;
            }
            context.HandleResponse();
            context.Response.Redirect("/Login");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy(Permissions.DataDelete, policy => policy.RequireClaim("Permission", Permissions.DataDelete));
    options.AddPolicy(Permissions.AdminAccess, policy => policy.RequireRole(AppRoles.Admin));
});

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Login");
});

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CadastreInvent API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

await app.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<CadastreDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var allPermissions = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.GetValue(null)?.ToString() ?? string.Empty)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var adminRole = db.Roles.Include(r => r.Permissions).FirstOrDefault(r => r.Name == AppRoles.Admin);
            if (adminRole == null)
            {
                adminRole = new Role(AppRoles.Admin, "Системный администратор платформы");
                db.Roles.Add(adminRole);
            }

            if (!adminRole.Permissions.Any())
            {
                foreach (var p in allPermissions) adminRole.AddPermission(p);
            }

            var employeeRole = db.Roles.Include(r => r.Permissions).FirstOrDefault(r => r.Name == AppRoles.Employee);
            if (employeeRole == null)
            {
                employeeRole = new Role(AppRoles.Employee, "Сотрудник управления регистрации");
                db.Roles.Add(employeeRole);
            }

            if (!employeeRole.Permissions.Any())
            {
                foreach (var p in allPermissions.Where(x => x != Permissions.AdminAccess && x != Permissions.DataDelete))
                    employeeRole.AddPermission(p);
            }

            var inspectorRole = db.Roles.Include(r => r.Permissions).FirstOrDefault(r => r.Name == AppRoles.Inspector);
            if (inspectorRole == null)
            {
                inspectorRole = new Role(AppRoles.Inspector, "Полевой инспектор (Мобильный доступ)");
                db.Roles.Add(inspectorRole);
            }

            if (!inspectorRole.Permissions.Any())
            {
                inspectorRole.AddPermission(Permissions.ExecuteFieldTasks);
                inspectorRole.AddPermission(Permissions.ViewGisMap);
            }

            db.SaveChanges();

            var adminEmail = "admin@cadastre.gov";
            if (!db.Users.Any(u => u.Email == adminEmail))
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin");
                var adminUser = new User("Системный Администратор", adminEmail, passwordHash, adminRole.Id);
                db.Users.Add(adminUser);
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Критическая ошибка при инициализации базовых данных (Seeder).");
            throw;
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CadastreInvent API v1"));
}

app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.MapHub<InspectionHub>("/hubs/inspection");
app.MapHub<MassAppraisalHub>("/hubs/mass-appraisal");

app.Run();