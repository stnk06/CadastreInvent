using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CadastreInvent.Shared.Application.Reports;

namespace CadastreInvent.Infrastructure.Reports
{
    public class DocumentGeneratorService : IDocumentGeneratorService
    {
        public byte[] GenerateZoningSummaryExcel(IEnumerable<ZoningSummaryDto> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Сводная аналитика по зонам");
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.SlateGray;
            headerRow.Style.Font.FontColor = XLColor.White;

            worksheet.Cell(1, 1).Value = "Оценочная зона";
            worksheet.Cell(1, 2).Value = "Количество объектов";
            worksheet.Cell(1, 3).Value = "Суммарная площадь (м²)";
            worksheet.Cell(1, 4).Value = "Общая кадастровая стоимость (₽)";

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.Zoning;
                worksheet.Cell(row, 2).Value = item.ObjectsCount;
                worksheet.Cell(row, 3).Value = item.TotalArea;
                worksheet.Cell(row, 4).Value = item.TotalValue;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₽";
                row++;
            }
            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateDataQualityExcel(IEnumerable<DataQualityDto> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Оценка полноты данных");
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.DarkRed;
            headerRow.Style.Font.FontColor = XLColor.White;

            worksheet.Cell(1, 1).Value = "Зона";
            worksheet.Cell(1, 2).Value = "Всего объектов";
            worksheet.Cell(1, 3).Value = "Отсутствуют координаты";
            worksheet.Cell(1, 4).Value = "Отсутствует площадь";
            worksheet.Cell(1, 5).Value = "Не указан год постройки";
            worksheet.Cell(1, 6).Value = "Процент нарушений";

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.Zoning;
                worksheet.Cell(row, 2).Value = item.TotalObjects;
                worksheet.Cell(row, 3).Value = item.MissingCoords;
                worksheet.Cell(row, 4).Value = item.MissingArea;
                worksheet.Cell(row, 5).Value = item.MissingYear;
                worksheet.Cell(row, 6).Value = item.DefectPercentage;
                row++;
            }
            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateInspectorKpiExcel(IEnumerable<InspectorKpiDto> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Эффективность инспекторов");
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.DarkGreen;
            headerRow.Style.Font.FontColor = XLColor.White;

            worksheet.Cell(1, 1).Value = "Инспектор (сотрудник)";
            worksheet.Cell(1, 2).Value = "Назначено задач";
            worksheet.Cell(1, 3).Value = "Успешно выполнено";
            worksheet.Cell(1, 4).Value = "Нарушен срок";
            worksheet.Cell(1, 5).Value = "Среднее время исполнения (часы)";

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.InspectorName;
                worksheet.Cell(row, 2).Value = item.AssignedTasks;
                worksheet.Cell(row, 3).Value = item.CompletedTasks;
                worksheet.Cell(row, 4).Value = item.OverdueTasks;
                worksheet.Cell(row, 5).Value = Math.Round(item.AvgTimeHours, 1);
                row++;
            }
            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateComparativeAnalysisExcel(IEnumerable<ComparativeAnalysisDto> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Сравнительный анализ отклонений");
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            headerRow.Style.Font.FontColor = XLColor.White;

            worksheet.Cell(1, 1).Value = "Объект недвижимости";
            worksheet.Cell(1, 2).Value = "Предыдущая оценка";
            worksheet.Cell(1, 3).Value = "Новая оценка";
            worksheet.Cell(1, 4).Value = "Разница в стоимости (₽)";
            worksheet.Cell(1, 5).Value = "Процент отклонения";

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.BaUnitName;
                worksheet.Cell(row, 2).Value = item.OldValue;
                worksheet.Cell(row, 3).Value = item.NewValue;
                worksheet.Cell(row, 4).Value = item.Difference;
                worksheet.Cell(row, 5).Value = Math.Round(item.DiffPercentage, 1);

                worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 ₽";
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 ₽";
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₽";
                row++;
            }
            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ОБНОВЛЕНИЕ: Встраивание фрагмента карты в PDF
        public byte[] GenerateCadastralExtractPdf(CadastralExtractDto extract, byte[] mapImageBytes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Column(header =>
                    {
                        header.Item().AlignCenter().Text("ВЫПИСКА ИЗ ГОСУДАРСТВЕННОГО РЕЕСТРА НЕДВИЖИМОСТИ")
                            .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);

                        header.Item().PaddingTop(5).AlignCenter().Text($"Сформировано: {DateTime.UtcNow:dd.MM.yyyy HH:mm} (UTC)")
                            .FontSize(9).FontColor(Colors.Grey.Medium);

                        header.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(15);

                        column.Item().Text("1. Общие сведения об объекте").SemiBold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => { columns.ConstantColumn(150); columns.RelativeColumn(); });
                            table.Cell().Text("Код объекта:").SemiBold(); table.Cell().Text(extract.BaUnitId.ToString());
                            table.Cell().Text("Наименование:").SemiBold(); table.Cell().Text(extract.BaUnitName);
                            table.Cell().Text("Оценочная зона:").SemiBold(); table.Cell().Text(extract.Zoning);
                        });

                        column.Item().Text("2. Пространственные и физические параметры").SemiBold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => { columns.ConstantColumn(150); columns.RelativeColumn(); });
                            table.Cell().Text("Кадастровые контуры:").SemiBold(); table.Cell().Text(extract.SpatialReferences);
                            table.Cell().Text("Установленная площадь:").SemiBold(); table.Cell().Text($"{extract.TotalArea:N1} м²");
                        });

                        column.Item().Text("3. Экономические и стоимостные показатели").SemiBold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => { columns.ConstantColumn(150); columns.RelativeColumn(); });
                            table.Cell().Text("Кадастровая стоимость:").SemiBold(); table.Cell().Text($"{extract.AssessedValue:N2} руб.").FontColor(Colors.Green.Darken2).SemiBold();
                            table.Cell().Text("Метод вычисления:").SemiBold(); table.Cell().Text(extract.Method);
                        });

                        column.Item().PaddingTop(20).Text("4. Координатное описание границ и ситуационный план").SemiBold().FontSize(14);

                        // Если есть байты карты, вставляем картинку перед координатами
                        if (mapImageBytes != null)
                        {
                            column.Item().PaddingBottom(15).AlignCenter().Width(400).Image(mapImageBytes).FitWidth();
                        }

                        // Вставляем красивый список точек вместо сплошного WKT
                        column.Item().Background(Colors.Grey.Lighten4).Padding(10)
                            .Text(extract.FormattedCoordinates).FontSize(9);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Официальный документ | Страница ");
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateGenericTablePdf(string title, string[] headers, string[][] rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Column(header =>
                    {
                        header.Item().AlignCenter().Text(title).SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);
                        header.Item().PaddingTop(5).AlignCenter().Text($"Сформировано: {DateTime.UtcNow:dd.MM.yyyy HH:mm} (UTC)").FontSize(9).FontColor(Colors.Grey.Medium);
                        header.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns => { for (int i = 0; i < headers.Length; i++) columns.RelativeColumn(); });

                        table.Header(header =>
                        {
                            foreach (var text in headers)
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text(text).SemiBold();
                            }
                        });

                        foreach (var row in rows)
                        {
                            foreach (var cellText in row)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(cellText);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Официальный отчет | Страница ");
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateDataQualityPdf(IEnumerable<DataQualityDto> data)
        {
            var rows = new List<string[]>();
            foreach (var d in data) rows.Add(new[] { d.Zoning, d.TotalObjects.ToString(), d.MissingCoords.ToString(), d.MissingArea.ToString(), d.MissingYear.ToString(), d.DefectPercentage + "%" });
            return GenerateGenericTablePdf("Отчет о качестве данных (Аномалии заполнения)", new[] { "Территориальная зона", "Всего объектов", "Без координат", "Без площади", "Без года постройки", "Процент нарушений" }, rows.ToArray());
        }

        public byte[] GenerateInspectorKpiPdf(IEnumerable<InspectorKpiDto> data)
        {
            var rows = new List<string[]>();
            foreach (var d in data) rows.Add(new[] { d.InspectorName, d.AssignedTasks.ToString(), d.CompletedTasks.ToString(), d.OverdueTasks.ToString(), Math.Round(d.AvgTimeHours, 1).ToString() });
            return GenerateGenericTablePdf("Показатели эффективности инспекторов", new[] { "Сотрудник", "Назначено поручений", "Успешно выполнено", "Нарушен срок", "Среднее время исполнения (ч)" }, rows.ToArray());
        }

        public byte[] GenerateComparativeAnalysisPdf(IEnumerable<ComparativeAnalysisDto> data)
        {
            var rows = new List<string[]>();
            foreach (var d in data) rows.Add(new[] { d.BaUnitName, d.OldValue.ToString("N2"), d.NewValue.ToString("N2"), d.Difference.ToString("N2"), Math.Round(d.DiffPercentage, 1) + "%" });
            return GenerateGenericTablePdf("Сравнительный анализ отклонений", new[] { "Имущественный объект", "Предыдущая оценка", "Новая оценка", "Разница", "Процент отклонения" }, rows.ToArray());
        }
    }
}