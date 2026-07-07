using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using ClosedXML.Excel;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoCheckAML.Api.Business
{
    public interface IExportService
    {
        byte[] ExportFormSubmissionsToExcel(List<FormSubmissionDto> submissions);
        byte[] ExportFormSubmissionsToPdf(List<FormSubmissionDto> submissions);
        byte[] ExportUsersToExcel(List<UserDto> users);
    }

    public class ExportService : IExportService
    {
        public byte[] ExportFormSubmissionsToExcel(List<FormSubmissionDto> submissions)
        {
            using var workbook = new XLWorkbook();

            // Hoja 1: Resumen de Inspecciones
            var wsResumen = workbook.Worksheets.Add("Resumen Inspecciones");

            // Headers principales
            var headers = new[] {
                "ID", "Placa", "Tipo Vehículo", "Estado", "Falla Detectada / Motivo Rechazo",
                "Ubicación", "Fecha Actividad", "Inspeccionado por", "Fecha Inspección",
                "Aprobado por Ing. Mecánico", "Fecha Aprobación Ing.",
                "Aprobado por Supervisor HSEQ", "Fecha Aprobación Sup.",
                "Observaciones Inspector"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = wsResumen.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x2E4057);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
            }

            // Data rows
            int row = 2;
            foreach (var s in submissions.OrderByDescending(f => f.CreatedAt))
            {
                // Buscar la placa en las respuestas
                var placaAnswer = s.Answers?.FirstOrDefault(a => a.FormFieldLabel?.Contains("Placa") == true);
                string placa = placaAnswer?.FieldValue ?? "-";

                wsResumen.Cell(row, 1).Value = s.Id;
                wsResumen.Cell(row, 2).Value = placa;
                wsResumen.Cell(row, 3).Value = s.VehicleTypeName ?? "-";

                // Estado con color
                var estadoCell = wsResumen.Cell(row, 4);
                estadoCell.Value = s.Status ?? "Pendiente";
                switch (s.Status)
                {
                    case "OPERATIVO":
                        estadoCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x28A745); // Verde
                        estadoCell.Style.Font.FontColor = XLColor.White;
                        estadoCell.Style.Font.Bold = true;
                        break;
                    case "INOPERATIVO":
                        estadoCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xDC3545); // Rojo
                        estadoCell.Style.Font.FontColor = XLColor.White;
                        estadoCell.Style.Font.Bold = true;
                        break;
                    case "EN REVISION":
                        estadoCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xFFC107); // Amarillo
                        estadoCell.Style.Font.FontColor = XLColor.Black;
                        estadoCell.Style.Font.Bold = true;
                        break;
                    default:
                        estadoCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x6C757D); // Gris
                        estadoCell.Style.Font.FontColor = XLColor.White;
                        break;
                }

                // Columna 5: Falla Detectada / Motivo Rechazo (resaltada si INOPERATIVO)
                var fallaCell = wsResumen.Cell(row, 5);
                fallaCell.Value = s.ObservationsByRectifier ?? "-";
                if (s.Status == "INOPERATIVO" && !string.IsNullOrEmpty(s.ObservationsByRectifier))
                {
                    fallaCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xFFE5E5); // Rojo claro
                    fallaCell.Style.Font.FontColor = XLColor.FromArgb(0xDC3545);
                    fallaCell.Style.Font.Bold = true;
                }

                wsResumen.Cell(row, 6).Value = s.ActivityLocation ?? "-";
                wsResumen.Cell(row, 7).Value = s.ActivityDate.ToString("yyyy-MM-dd");
                wsResumen.Cell(row, 8).Value = s.SubmittedByUserName ?? "-";
                wsResumen.Cell(row, 9).Value = s.CreatedAt.ToString("yyyy-MM-dd HH:mm");

                // Aprobaciones
                wsResumen.Cell(row, 10).Value = s.ApprovedByIngenieroId.HasValue ? "✓ Aprobado" : "✗ Pendiente";
                wsResumen.Cell(row, 11).Value = s.ApprovedByIngenieroAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
                wsResumen.Cell(row, 12).Value = s.ApprovedBySupervisorId.HasValue ? "✓ Aprobado" : "✗ Pendiente";
                wsResumen.Cell(row, 13).Value = s.ApprovedBySupervisorAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";

                wsResumen.Cell(row, 14).Value = s.ObservationsByRespondent ?? "-";

                // Fila par - fondo gris claro
                if (row % 2 == 0)
                {
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        // No aplicar a columna Estado (4) ni Falla Detectada (5) si tiene color propio
                        if (col != 4 && !(col == 5 && s.Status == "INOPERATIVO" && !string.IsNullOrEmpty(s.ObservationsByRectifier)))
                            wsResumen.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF8F9FA);
                    }
                }

                row++;
            }

            // Auto-ajustar columnas
            wsResumen.Columns(1, 14).AdjustToContents();
            wsResumen.SheetView.FreezeRows(1);
            wsResumen.SheetView.FreezeColumns(3); // Congelar hasta la columna de placa

            // Hoja 2: Detalle de Respuestas (todas las inspecciones con sus respuestas)
            var wsDetalle = workbook.Worksheets.Add("Detalle Respuestas");

            var detalleHeaders = new[] { "ID Inspección", "Placa", "Estado", "Campo", "Respuesta", "Notas" };
            for (int i = 0; i < detalleHeaders.Length; i++)
            {
                var cell = wsDetalle.Cell(1, i + 1);
                cell.Value = detalleHeaders[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x2E4057);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int detalleRow = 2;
            foreach (var s in submissions.OrderByDescending(f => f.CreatedAt))
            {
                var placaAnswer = s.Answers?.FirstOrDefault(a => a.FormFieldLabel?.Contains("Placa") == true);
                string placa = placaAnswer?.FieldValue ?? "-";

                if (s.Answers?.Any() == true)
                {
                    foreach (var answer in s.Answers.OrderBy(a => a.FormFieldId))
                    {
                        wsDetalle.Cell(detalleRow, 1).Value = s.Id;
                        wsDetalle.Cell(detalleRow, 2).Value = placa;
                        wsDetalle.Cell(detalleRow, 3).Value = s.Status ?? "Pendiente";
                        wsDetalle.Cell(detalleRow, 4).Value = answer.FormFieldLabel ?? $"Campo {answer.FormFieldId}";
                        wsDetalle.Cell(detalleRow, 5).Value = answer.FieldValue ?? "-";
                        wsDetalle.Cell(detalleRow, 6).Value = answer.Notes ?? "-";

                        // Resaltar respuestas con problemas (NO, FALLO)
                        if (answer.FieldValue?.Equals("NO", StringComparison.OrdinalIgnoreCase) == true ||
                            answer.FieldValue?.Equals("FALLO", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            wsDetalle.Cell(detalleRow, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(0xFFE5E5);
                            wsDetalle.Cell(detalleRow, 5).Style.Font.Bold = true;
                            wsDetalle.Cell(detalleRow, 5).Style.Font.FontColor = XLColor.FromArgb(0xDC3545);
                        }

                        if (detalleRow % 2 == 0)
                        {
                            for (int col = 1; col <= detalleHeaders.Length; col++)
                            {
                                if (answer.FieldValue?.Equals("NO", StringComparison.OrdinalIgnoreCase) != true &&
                                    answer.FieldValue?.Equals("FALLO", StringComparison.OrdinalIgnoreCase) != true)
                                {
                                    wsDetalle.Cell(detalleRow, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF8F9FA);
                                }
                            }
                        }

                        detalleRow++;
                    }
                }
            }

            wsDetalle.Columns().AdjustToContents();
            wsDetalle.SheetView.FreezeRows(1);
            wsDetalle.SheetView.FreezeColumns(3);

            // Hoja 3: Estadísticas
            var wsEstadisticas = workbook.Worksheets.Add("Estadísticas");

            int statsRow = 1;

            // Título
            var titleCell = wsEstadisticas.Cell(statsRow, 1);
            titleCell.Value = "ESTADÍSTICAS DE INSPECCIONES";
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 16;
            wsEstadisticas.Range(statsRow, 1, statsRow, 3).Merge();
            statsRow += 2;

            // Estadísticas por estado
            var operativos = submissions.Count(s => s.Status == "OPERATIVO");
            var inoperativos = submissions.Count(s => s.Status == "INOPERATIVO");
            var enRevision = submissions.Count(s => s.Status == "EN REVISION");
            var pendientes = submissions.Count(s => s.Status == "Pendiente" || string.IsNullOrEmpty(s.Status));

            wsEstadisticas.Cell(statsRow, 1).Value = "Estado";
            wsEstadisticas.Cell(statsRow, 2).Value = "Cantidad";
            wsEstadisticas.Cell(statsRow, 3).Value = "Porcentaje";
            wsEstadisticas.Range(statsRow, 1, statsRow, 3).Style.Fill.BackgroundColor = XLColor.FromArgb(0x2E4057);
            wsEstadisticas.Range(statsRow, 1, statsRow, 3).Style.Font.FontColor = XLColor.White;
            wsEstadisticas.Range(statsRow, 1, statsRow, 3).Style.Font.Bold = true;
            statsRow++;

            void AddStatRow(string label, int count, int total, XLColor color)
            {
                wsEstadisticas.Cell(statsRow, 1).Value = label;
                wsEstadisticas.Cell(statsRow, 2).Value = count;
                wsEstadisticas.Cell(statsRow, 3).Value = total > 0 ? $"{(count * 100.0 / total):F1}%" : "0%";
                wsEstadisticas.Cell(statsRow, 1).Style.Fill.BackgroundColor = color;
                wsEstadisticas.Cell(statsRow, 1).Style.Font.FontColor = XLColor.White;
                wsEstadisticas.Cell(statsRow, 1).Style.Font.Bold = true;
                statsRow++;
            }

            int total = submissions.Count;
            AddStatRow("OPERATIVO", operativos, total, XLColor.FromArgb(0x28A745));
            AddStatRow("INOPERATIVO", inoperativos, total, XLColor.FromArgb(0xDC3545));
            AddStatRow("EN REVISIÓN", enRevision, total, XLColor.FromArgb(0xFFC107));
            AddStatRow("PENDIENTE", pendientes, total, XLColor.FromArgb(0x6C757D));

            statsRow += 2;
            wsEstadisticas.Cell(statsRow, 1).Value = "TOTAL INSPECCIONES:";
            wsEstadisticas.Cell(statsRow, 1).Style.Font.Bold = true;
            wsEstadisticas.Cell(statsRow, 2).Value = total;
            wsEstadisticas.Cell(statsRow, 2).Style.Font.Bold = true;

            wsEstadisticas.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportFormSubmissionsToPdf(List<FormSubmissionDto> submissions)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // Header
                    page.Header().Height(60).Background(Colors.Blue.Darken3).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("REPORTE DE INSPECCIONES VEHICULARES").FontSize(16).Bold().FontColor(Colors.White);
                            col.Item().Text($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Lighten2);
                        });
                        row.ConstantItem(100).AlignRight().Text($"Total: {submissions.Count}").FontSize(12).Bold().FontColor(Colors.White);
                    });

                    // Content
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // Tabla de inspecciones
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // ID
                                columns.ConstantColumn(70);  // Placa
                                columns.ConstantColumn(80);  // Tipo
                                columns.ConstantColumn(70);  // Estado
                                columns.RelativeColumn();    // Ubicación
                                columns.ConstantColumn(70);  // Fecha
                                columns.RelativeColumn();    // Inspector
                                columns.ConstantColumn(80);  // Ing. Mecánico
                                columns.ConstantColumn(80);  // Supervisor HSEQ
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("ID").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Placa").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Tipo Vehículo").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Estado").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Ubicación").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Fecha").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Inspeccionado por").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Ing. Mecánico").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Supervisor HSEQ").Bold().FontColor(Colors.White);
                            });

                            // Rows
                            int rowIndex = 0;
                            foreach (var s in submissions.OrderByDescending(f => f.CreatedAt))
                            {
                                var placaAnswer = s.Answers?.FirstOrDefault(a => a.FormFieldLabel?.Contains("Placa") == true);
                                string placa = placaAnswer?.FieldValue ?? "-";
                                var backgroundColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                // Color del estado
                                string statusColor = s.Status switch
                                {
                                    "OPERATIVO" => "#28A745",
                                    "INOPERATIVO" => "#DC3545",
                                    "EN REVISION" => "#FFC107",
                                    _ => "#6C757D"
                                };

                                table.Cell().Background(backgroundColor).Padding(5).Text(s.Id.ToString());
                                table.Cell().Background(backgroundColor).Padding(5).Text(placa).Bold();
                                table.Cell().Background(backgroundColor).Padding(5).Text(s.VehicleTypeName ?? "-");
                                table.Cell().Background(statusColor).Padding(5).Text(s.Status ?? "Pendiente").Bold().FontColor(Colors.White);
                                table.Cell().Background(backgroundColor).Padding(5).Text(s.ActivityLocation ?? "-");
                                table.Cell().Background(backgroundColor).Padding(5).Text(s.ActivityDate.ToString("yyyy-MM-dd"));
                                table.Cell().Background(backgroundColor).Padding(5).Text(s.SubmittedByUserName ?? "-");
                                table.Cell().Background(backgroundColor).Padding(5).Text(s.ApprovedByIngenieroId.HasValue ? "✓" : "✗");
                                table.Cell().Background(backgroundColor).Padding(5).Text(s.ApprovedBySupervisorId.HasValue ? "✓" : "✗");

                                rowIndex++;
                            }
                        });

                        col.Item().PaddingTop(20).Row(row =>
                        {
                            var operativos = submissions.Count(s => s.Status == "OPERATIVO");
                            var inoperativos = submissions.Count(s => s.Status == "INOPERATIVO");
                            var enRevision = submissions.Count(s => s.Status == "EN REVISION");
                            var pendientes = submissions.Count(s => s.Status == "Pendiente" || string.IsNullOrEmpty(s.Status));

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("RESUMEN DE ESTADOS").FontSize(12).Bold();
                                col.Item().PaddingTop(5).Row(r =>
                                {
                                    r.AutoItem().Width(120).Background("#28A745").Padding(5).Text($"OPERATIVO: {operativos}").FontColor(Colors.White).Bold();
                                    r.AutoItem().PaddingLeft(5).Width(120).Background("#DC3545").Padding(5).Text($"INOPERATIVO: {inoperativos}").FontColor(Colors.White).Bold();
                                    r.AutoItem().PaddingLeft(5).Width(120).Background("#FFC107").Padding(5).Text($"EN REVISIÓN: {enRevision}").FontColor(Colors.Black).Bold();
                                    r.AutoItem().PaddingLeft(5).Width(120).Background("#6C757D").Padding(5).Text($"PENDIENTE: {pendientes}").FontColor(Colors.White).Bold();
                                });
                            });
                        });
                    });

                    // Footer
                    page.Footer().Height(30).Background(Colors.Grey.Lighten3).Padding(10).AlignCenter().Text(text =>
                    {
                        text.Span("AutoCheckAML - Sistema de Inspección Vehicular | ").FontSize(8);
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        public byte[] ExportUsersToExcel(List<UserDto> users)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Usuarios");

            var headers = new[] { "ID", "Usuario", "Email", "Nombre Completo", "Roles", "Cuadrilla", "Activo", "Último Login" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            var headerRow = ws.Row(1);
            headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0x2E4057);
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var u in users)
            {
                ws.Cell(row, 1).Value = u.Id;
                ws.Cell(row, 2).Value = u.Username;
                ws.Cell(row, 3).Value = u.Email;
                ws.Cell(row, 4).Value = u.FullName;
                ws.Cell(row, 5).Value = string.Join(", ", u.Roles ?? new List<string>());
                ws.Cell(row, 6).Value = u.CrewName ?? "-";
                ws.Cell(row, 7).Value = u.IsActive ? "Sí" : "No";
                ws.Cell(row, 8).Value = u.LastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "-";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(0xEBF0F8);

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
