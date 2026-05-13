using System;
using MediatR;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.Commands
{
    public class RegisterPartyCommand : IRequest<Guid>
    {
        public PartyType Type { get; set; }
        public string GovRegNumType { get; set; } = string.Empty;
        public string ExtId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ContactType { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }
}