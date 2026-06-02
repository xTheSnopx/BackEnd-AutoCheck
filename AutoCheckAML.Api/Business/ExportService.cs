using AutoCheckAML.Api.Entity;
using ClosedXML.Excel;

namespace AutoCheckAML.Api.Business
{
    public interface IExportService
    {
        byte[] ExportToExcel(List<FormSubmission> forms, string fileName = "formularios");
    }

    public class ExportService : IExportService
    {
        public byte[] ExportToExcel(List<FormSubmission> forms, string fileName = "formularios")
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Formularios");

                    // Encabezados
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Nombre";
                    worksheet.Cell(1, 3).Value = "Email";
                    worksheet.Cell(1, 4).Value = "Teléfono";
                    worksheet.Cell(1, 5).Value = "Empresa";
                    worksheet.Cell(1, 6).Value = "Asunto";
                    worksheet.Cell(1, 7).Value = "Mensaje";
                    worksheet.Cell(1, 8).Value = "Fecha";
                    worksheet.Cell(1, 9).Value = "Estado";
                    worksheet.Cell(1, 10).Value = "Fecha de Registro";

                    // Aplicar estilos a encabezados
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0x4472C4);
                    headerRow.Style.Font.FontColor = XLColor.White;
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // Agregar datos
                    int row = 2;
                    foreach (var form in forms.OrderByDescending(f => f.CreatedAt))
                    {
                        worksheet.Cell(row, 1).Value = form.Id;
                        worksheet.Cell(row, 2).Value = form.Nombre;
                        worksheet.Cell(row, 3).Value = form.Email;
                        worksheet.Cell(row, 4).Value = form.Telefono;
                        worksheet.Cell(row, 5).Value = form.Empresa;
                        worksheet.Cell(row, 6).Value = form.Asunto;
                        worksheet.Cell(row, 7).Value = form.Mensaje;
                        worksheet.Cell(row, 8).Value = form.Fecha.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 9).Value = form.Status;
                        worksheet.Cell(row, 10).Value = form.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

                        // Aplicar estilos a filas de datos
                        var dataRow = worksheet.Row(row);
                        if (row % 2 == 0)
                        {
                            dataRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0xEBF0F8);
                        }
                        dataRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                        dataRow.Style.Alignment.WrapText = true;

                        row++;
                    }

                    // Ajustar ancho de columnas
                    worksheet.Column(1).Width = 8;
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 25;
                    worksheet.Column(4).Width = 15;
                    worksheet.Column(5).Width = 20;
                    worksheet.Column(6).Width = 25;
                    worksheet.Column(7).Width = 40;
                    worksheet.Column(8).Width = 15;
                    worksheet.Column(9).Width = 15;
                    worksheet.Column(10).Width = 20;

                    // Congelar encabezado
                    worksheet.SheetView.FreezeRows(1);

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar a Excel: {ex.Message}");
            }
        }
    }
}
