using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using ClosedXML.Excel;

namespace AutoCheckAML.Api.Business
{
    public interface IExportService
    {
        byte[] ExportFormSubmissionsToExcel(List<FormSubmissionDto> submissions);
        byte[] ExportUsersToExcel(List<UserDto> users);
    }

    public class ExportService : IExportService
    {
        public byte[] ExportFormSubmissionsToExcel(List<FormSubmissionDto> submissions)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Formularios");

            // Headers
            var headers = new[] { "ID", "Plantilla", "Respondido por", "Cuadrilla", "Ubicación", "Fecha Actividad", "Estado", "Verificado por", "Fecha Registro" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            // Style headers
            var headerRow = ws.Row(1);
            headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0x2E4057);
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data rows
            int row = 2;
            foreach (var s in submissions.OrderByDescending(f => f.CreatedAt))
            {
                ws.Cell(row, 1).Value = s.Id;
                ws.Cell(row, 2).Value = s.FormTemplateName ?? "-";
                ws.Cell(row, 3).Value = s.SubmittedByUserName ?? "-";
                ws.Cell(row, 4).Value = s.AssignedToCrewName ?? "-";
                ws.Cell(row, 5).Value = s.ActivityLocation ?? "-";
                ws.Cell(row, 6).Value = s.ActivityDate.ToString("yyyy-MM-dd");
                ws.Cell(row, 7).Value = s.Status;
                ws.Cell(row, 8).Value = s.VerifiedByUserName ?? "-";
                ws.Cell(row, 9).Value = s.CreatedAt.ToString("yyyy-MM-dd HH:mm");

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(0xEBF0F8);

                row++;
            }

            // Auto-fit columns
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
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
