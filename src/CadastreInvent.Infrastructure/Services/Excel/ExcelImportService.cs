using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Infrastructure.Services.Excel
{
    public class ExcelImportService : IExcelImportService
    {
        public ExcelPreviewResultDto ParseExcelFile(Stream fileStream)
        {
            var result = new ExcelPreviewResultDto
            {
                SessionId = Guid.NewGuid()
            };

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                throw new InvalidOperationException("Загруженный Excel-файл не содержит листов.");
            }

            var rows = worksheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var parsedRow = new ExcelParsedRowDto
                {
                    RowIndex = row.RowNumber(),
                    IsValid = true
                };

                try
                {
                    parsedRow.CadastralNumber = row.Cell(1).GetString().Trim();
                    if (string.IsNullOrEmpty(parsedRow.CadastralNumber))
                    {
                        parsedRow.IsValid = false;
                        parsedRow.ErrorMessage += "Отсутствует кадастровый номер. ";
                    }

                    var typeString = row.Cell(2).GetString().Trim();
                    if (Enum.TryParse<SpatialUnitType>(typeString, true, out var parsedType))
                    {
                        parsedRow.Type = parsedType;
                    }
                    else
                    {
                        parsedRow.Type = SpatialUnitType.Parcel;
                    }

                    if (row.Cell(3).TryGetValue<double>(out var area) && area > 0)
                    {
                        parsedRow.AreaSqMeters = area;
                    }
                    else
                    {
                        parsedRow.IsValid = false;
                        parsedRow.ErrorMessage += "Некорректная площадь. ";
                    }

                    parsedRow.Address = row.Cell(4).GetString().Trim();
                    if (string.IsNullOrEmpty(parsedRow.Address))
                    {
                        parsedRow.Address = $"Объект недвижимости ({parsedRow.CadastralNumber})";
                    }

                    parsedRow.Wkt = row.Cell(5).GetString().Trim();
                    if (string.IsNullOrEmpty(parsedRow.Wkt) || !parsedRow.Wkt.StartsWith("POLYGON"))
                    {
                        parsedRow.IsValid = false;
                        parsedRow.ErrorMessage += "Отсутствует или некорректный WKT полигон. ";
                    }
                }
                catch (Exception ex)
                {
                    parsedRow.IsValid = false;
                    parsedRow.ErrorMessage += $"Критическая ошибка парсинга: {ex.Message}";
                }

                result.PreviewData.Add(parsedRow);

                if (parsedRow.IsValid) result.ValidRows++;
                else result.InvalidRows++;
            }

            result.TotalRows = result.PreviewData.Count;
            return result;
        }
    }
}