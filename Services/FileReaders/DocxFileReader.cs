using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.Services.FileReaders;

public class DocxFileReader : IFileReader
{
    public bool CanRead(string filePath) =>
        filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ReadTextAsync(string filePath, IProgress<string>? progress = null)
    {
        return await Task.Run(() =>
        {
            using var wordDocument = WordprocessingDocument.Open(filePath, false);
            var body = wordDocument.MainDocumentPart?.Document?.Body;
            
            if (body == null) 
                return string.Empty;

            var text = string.Join(Environment.NewLine, body.Elements<Paragraph>()
                .Select(p => p.InnerText ?? string.Empty));
                
            progress?.Report("DOCX обработан");
            return text;
        });
    }
}