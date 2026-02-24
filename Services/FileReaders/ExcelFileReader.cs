#pragma warning disable CS0618
using OfficeOpenXml;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using System.IO;
using System.Text;

namespace DeepSeekSurveyAnalyzer.Services.FileReaders;

public class ExcelFileReader : IFileReader
{
    public bool CanRead(string filePath) =>
        filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ReadTextAsync(string filePath, IProgress<string>? progress = null)
    {
        return await Task.Run(() =>
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(filePath));
            var text = new StringBuilder();
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                if (worksheet.Dimension == null) continue;
                text.AppendLine($"--- Лист: {worksheet.Name} ---");
                for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                {
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Text;
                        if (!string.IsNullOrWhiteSpace(cellValue))
                            text.Append(cellValue + "\t");
                    }
                    text.AppendLine();
                }
                progress?.Report($"Обработан лист {worksheet.Name}");
            }
            return text.ToString();
        });
    }
}
#pragma warning restore CS0618