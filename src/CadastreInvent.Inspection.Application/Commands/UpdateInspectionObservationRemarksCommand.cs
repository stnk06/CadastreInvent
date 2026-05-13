using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record UpdateInspectionObservationRemarksCommand(
        Guid ObservationId,
        string RemarksJson) : IRequest<bool>;

    public class UpdateInspectionObservationRemarksCommandHandler : IRequestHandler<UpdateInspectionObservationRemarksCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public UpdateInspectionObservationRemarksCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateInspectionObservationRemarksCommand request, CancellationToken cancellationToken)
        {
            var obs = await _dbContext.InspectionObservations.FindAsync(new object[] { request.ObservationId }, cancellationToken);

            if (obs == null) throw new Exception("Акт наблюдения не найден в базе данных.");

            string validRemarksJson = request.RemarksJson;
            if (!string.IsNullOrWhiteSpace(validRemarksJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(validRemarksJson);
                }
                catch (JsonException)
                {
                    validRemarksJson = JsonSerializer.Serialize(new { notes = request.RemarksJson });
                }
            }
            else
            {
                validRemarksJson = "{}";
            }

            obs.UpdateRemarks(validRemarksJson);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}