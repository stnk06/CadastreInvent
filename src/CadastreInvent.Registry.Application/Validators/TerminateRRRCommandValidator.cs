using System;
using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class TerminateRRRCommandValidator : AbstractValidator<TerminateRRRCommand>
    {
        public TerminateRRRCommandValidator()
        {
            RuleFor(x => x.BAUnitId).NotEmpty();
            RuleFor(x => x.RRRId).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        }
    }
}