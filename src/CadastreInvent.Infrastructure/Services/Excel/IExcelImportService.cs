using System.IO;

namespace CadastreInvent.Infrastructure.Services.Excel
{
    public interface IExcelImportService
    {
        ExcelPreviewResultDto ParseExcelFile(Stream fileStream);
    }
}