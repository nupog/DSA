using UglyToad.PdfPig;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using System.Text;

namespace DeepSeekSurveyAnalyzer.Services.FileReaders;

public class PdfFileReader : IFileReader
{
    public bool CanRead(string filePath) =>
        filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ReadTextAsync(string filePath, IProgress<string>? progress = null)
    {
        return await Task.Run(() =>
        {
            using var pdf = PdfDocument.Open(filePath);
            int totalPages = pdf.NumberOfPages;
            int currentPage = 0;
            var text = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                text.AppendLine(page.Text);
                currentPage++;
                progress?.Report($"Чтение PDF: страница {currentPage} из {totalPages}");
            }
            return text.ToString();
        });
    }
}