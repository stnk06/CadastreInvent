using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CadastreInvent.Shared.Application.Exceptions;

namespace CadastreInvent.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = Guid.NewGuid().ToString();
                _logger.LogCritical(ex, "{TraceId}|{Path}|{Message}", traceId, context.Request.Path, ex.Message);
                await HandleExceptionAsync(context, ex, traceId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                };

                switch (exception)
                {
                    case ValidationException validationEx:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        problemDetails.Title = "ValidationFailed";
                        problemDetails.Status = (int)HttpStatusCode.BadRequest;
                        problemDetails.Detail = "Validation error.";
                        problemDetails.Extensions["errors"] = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                        break;
                    case UnauthorizedException unauthorizedEx:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        problemDetails.Title = "Unauthorized";
                        problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                        problemDetails.Detail = unauthorizedEx.Message;
                        break;
                    case NotFoundException notFoundEx:
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        problemDetails.Title = "NotFound";
                        problemDetails.Status = (int)HttpStatusCode.NotFound;
                        problemDetails.Detail = notFoundEx.Message;
                        break;
                    case DbUpdateException:
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        problemDetails.Title = "DatabaseConflict";
                        problemDetails.Status = (int)HttpStatusCode.Conflict;
                        problemDetails.Detail = "Data integrity violation.";
                        break;
                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        problemDetails.Title = "InternalServerError";
                        problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                        problemDetails.Detail = "System failure.";
                        break;
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
            else
            {
                context.Response.Redirect($"/Error?traceId={traceId}");
            }
        }
    }
}