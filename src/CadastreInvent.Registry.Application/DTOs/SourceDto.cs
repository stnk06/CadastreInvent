using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.DTOs
{
    public class SourceDto
    {
        public Guid Id { get; set; }
        public SourceType Type { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string ContentUrl { get; set; } = string.Empty;
    }
}