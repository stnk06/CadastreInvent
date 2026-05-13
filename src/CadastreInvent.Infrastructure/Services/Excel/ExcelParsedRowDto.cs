using System;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Infrastructure.Services.Excel
{
    public class ExcelParsedRowDto
    {
        public int RowIndex { get; set; }
        public string CadastralNumber { get; set; } = string.Empty;
        public SpatialUnitType Type { get; set; }
        public double AreaSqMeters { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Wkt { get; set; } = string.Empty;

        public bool IsValid { get; set; }
        public bool IsDuplicate { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}