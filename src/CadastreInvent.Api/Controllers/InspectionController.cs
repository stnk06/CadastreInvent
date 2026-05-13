using System;
using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CadastreInvent.Api.Auth;
using CadastreInvent.Inspection.Application.Commands;
using CadastreInvent.Inspection.Application.Queries;
using CadastreInvent.Shared.Application.Auth;
using CadastreInvent.Shared.Application.Interfaces;
using CadastreInvent.Inspection.Domain.Enums;

namespace CadastreInvent.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;

        public InspectionController(IMediator mediator, ICurrentUserService currentUserService, IFileStorageService fileStorageService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
        }

        [HasPermission(Permissions.ManageFieldTasks)]
        [HttpGet("desktop/tasks")]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _mediator.Send(new GetDispatcherTasksQuery());
            return Ok(tasks);
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpGet("mobile/my-tasks")]
        public async Task<IActionResult> GetMyMobileTasks()
        {
            var tasks = await _mediator.Send(new GetMyMobileTasksQuery(_currentUserService.UserId));
            return Ok(tasks);
        }

        [HasPermission(Permissions.ManageFieldTasks)]
        [HttpGet("spatial-units/search")]
        public async Task<IActionResult> SearchSpatialUnits([FromQuery] string? q = null)
        {
            var results = await _mediator.Send(new SearchSpatialUnitsQuery(q));
            return Ok(results);
        }

        [HasPermission(Permissions.ManageFieldTasks)]
        [HttpPost("tasks")]
        public async Task<IActionResult> CreateTask([FromBody] CreateInspectionTaskCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                return Ok(new { id });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPut("tasks/{id}/start")]
        public async Task<IActionResult> StartTask(Guid id)
        {
            await _mediator.Send(new StartInspectionTaskCommand(id));
            return NoContent();
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPut("tasks/{id}/complete")]
        public async Task<IActionResult> CompleteTask(Guid id)
        {
            await _mediator.Send(new CompleteInspectionTaskCommand(id));
            return NoContent();
        }

        [HasPermission(Permissions.ManageFieldTasks)]
        [HttpPut("tasks/{id}/approve")]
        public async Task<IActionResult> ApproveTask(Guid id)
        {
            await _mediator.Send(new ApproveInspectionTaskCommand(id));
            return NoContent();
        }

        [HasPermission(Permissions.ManageFieldTasks)]
        [HttpPut("tasks/{id}/rework")]
        public async Task<IActionResult> SendForRework(Guid id, [FromBody] ReworkRequest request)
        {
            await _mediator.Send(new SendTaskForReworkCommand(id, request.Reason));
            return NoContent();
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPut("tasks/{id}/result")]
        public async Task<IActionResult> SaveResult(Guid id, [FromBody] SaveResultRequest request)
        {
            await _mediator.Send(new SaveInspectionResultCommand(id, request.Status, request.Conclusion));
            return NoContent();
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPost("observations")]
        public async Task<IActionResult> AddObservation([FromBody] AddInspectionObservationCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPut("observations/{id}/remarks")]
        public async Task<IActionResult> UpdateObservationRemarks(Guid id, [FromBody] string remarksJson)
        {
            await _mediator.Send(new UpdateInspectionObservationRemarksCommand(id, remarksJson));
            return NoContent();
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPost("mobile/photos/presign")]
        public async Task<IActionResult> GetPresignedUrl([FromBody] PresignPhotoRequest request)
        {
            var url = await _fileStorageService.GeneratePreSignedUrlAsync(request.FileName, request.ContentType, HttpContext.RequestAborted);
            var uri = new Uri(url);
            var finalUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}{uri.AbsolutePath}";
            return Ok(new { UploadUrl = url, FinalUrl = finalUrl });
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPost("mobile/photos/metadata")]
        public async Task<IActionResult> UploadMobilePhotoMetadata([FromBody] UploadMobilePhotoMetadataRequest request)
        {
            var cmd = new AddInspectionPhotoCommand(
                request.InspectionTaskId,
                request.PhotoUrl,
                request.Longitude,
                request.Latitude,
                request.Azimuth,
                request.CaptureDate,
                request.AppLocalId);

            var id = await _mediator.Send(cmd);
            return Ok(new { PhotoId = id, Url = request.PhotoUrl });
        }

        [AllowAnonymous]
        [HttpGet("forms/templates")]
        public IActionResult GetFormTemplates()
        {
            var templates = new[]
            {
                new
                {
                    category = "BoundaryVerification",
                    fields = new object[]
                    {
                        new { id = "Area", type = "number", label = "Фактическая площадь (кв.м)", required = true },
                        new { id = "Floor", type = "number", label = "Этажность", required = true },
                        new { id = "YearBuilt", type = "number", label = "Год постройки", required = true },
                        new { id = "DistanceToCenterKm", type = "number", label = "Удаленность от центра (км)", required = true },
                        new { id = "RoomsCount", type = "number", label = "Количество комнат", required = true },
                        new { id = "Notes", type = "text", label = "Дополнительные замечания", required = false }
                    }
                },
                new
                {
                    category = "ConditionAssessment",
                    fields = new object[]
                    {
                        new { id = "Condition", type = "select", label = "Состояние", options = new[] { "Хорошее", "Удовлетворительное", "Аварийное", "Разрушено" }, required = true },
                        new { id = "Notes", type = "text", label = "Описание повреждений", required = false }
                    }
                },
                new
                {
                    category = "DiscrepancyFound",
                    fields = new object[]
                    {
                        new { id = "Area", type = "number", label = "Фактическая площадь (кв.м)", required = true },
                        new { id = "DiscrepancyType", type = "select", label = "Характер расхождения", options = new[] { "Самозахват", "Наложение границ", "Снос объекта" }, required = true },
                        new { id = "Notes", type = "text", label = "Пояснения инспектора", required = true }
                    }
                },
                new
                {
                    category = "IllegalConstruction",
                    fields = new object[]
                    {
                        new { id = "Area", type = "number", label = "Площадь самозастроя (кв.м)", required = true },
                        new { id = "Floor", type = "number", label = "Этажность незаконного объекта", required = true },
                        new { id = "Notes", type = "text", label = "Описание постройки", required = true }
                    }
                }
            };

            return Ok(templates);
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPost("mobile/tasks/{id}/start")]
        public async Task<IActionResult> MobileStartTask(Guid id)
        {
            await _mediator.Send(new StartInspectionTaskCommand(id));
            return NoContent();
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPost("mobile/tasks/{id}/complete")]
        public async Task<IActionResult> MobileCompleteTask(Guid id)
        {
            await _mediator.Send(new CompleteInspectionTaskCommand(id));
            return NoContent();
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPost("mobile/sync/observations")]
        public async Task<IActionResult> MobileAddObservation([FromBody] MobileObservationRequest request)
        {
            var cmd = new AddInspectionObservationCommand(
                request.InspectionTaskId,
                (ObservationCategory)request.Category,
                request.RemarksJson,
                request.ObservationDate,
                request.AppLocalId);
            var id = await _mediator.Send(cmd);
            return Ok(new { id });
        }

        [HasPermission(Permissions.ExecuteFieldTasks)]
        [HttpPost("mobile/sync/photos")]
        public async Task<IActionResult> MobileUploadPhoto([FromForm] MobilePhotoUploadRequest request)
        {
            if (request.Photo == null || request.Photo.Length == 0)
                return BadRequest("Фотография не передана.");

            var photoUrl = await _fileStorageService.UploadFileAsync(request.Photo.OpenReadStream(), request.Photo.FileName, request.Photo.ContentType, HttpContext.RequestAborted);

            var cmd = new AddInspectionPhotoCommand(
                request.TaskId,
                photoUrl,
                request.Lon,
                request.Lat,
                request.Azimuth,
                request.CaptureDate,
                request.AppLocalId);

            var photoId = await _mediator.Send(cmd);
            return Ok(new { PhotoId = photoId, Url = photoUrl });
        }

        public class MobilePhotoUploadRequest
        {
            public Guid TaskId { get; set; }
            public double Lon { get; set; }
            public double Lat { get; set; }
            public double Azimuth { get; set; }
            public DateTime CaptureDate { get; set; }
            public Guid AppLocalId { get; set; }
            public IFormFile Photo { get; set; } = null!;
        }

        public class MobileObservationRequest
        {
            public Guid InspectionTaskId { get; set; }
            public int Category { get; set; }
            public string RemarksJson { get; set; } = string.Empty;
            public DateTime ObservationDate { get; set; }
            public Guid? AppLocalId { get; set; }
        }

        public class SaveResultRequest
        {
            public ViolationStatus Status { get; set; }
            public string Conclusion { get; set; } = string.Empty;
        }

        public class ReworkRequest
        {
            public string Reason { get; set; } = string.Empty;
        }

        public class PresignPhotoRequest
        {
            public string FileName { get; set; } = string.Empty;
            public string ContentType { get; set; } = string.Empty;
        }

        public class UploadMobilePhotoMetadataRequest
        {
            public Guid InspectionTaskId { get; set; }
            public string PhotoUrl { get; set; } = string.Empty;
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public double Azimuth { get; set; }
            public DateTime CaptureDate { get; set; }
            public Guid? AppLocalId { get; set; }
        }
    }
}