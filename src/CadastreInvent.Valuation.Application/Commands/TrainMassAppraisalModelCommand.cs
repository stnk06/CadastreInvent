using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record TrainMassAppraisalModelCommand(
        string Version,
        string Description) : IRequest<Guid>;
}