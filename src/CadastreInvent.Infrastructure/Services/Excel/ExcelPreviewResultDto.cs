using System;
using System.Collections.Generic;

namespace CadastreInvent.Infrastructure.Services.Excel
{
    public class ExcelPreviewResultDto
    {
        public Guid SessionId { get; set; }
        public string FileName { get; set; } = string.Empty;

        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }

        public int NewRows { get; set; }
        public int DuplicateRows { get; set; }

        public List<ExcelParsedRowDto> PreviewData { get; set; } = new();
    }
}