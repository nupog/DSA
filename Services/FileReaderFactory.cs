using DeepSeekSurveyAnalyzer.Services.Abstractions;
using DeepSeekSurveyAnalyzer.Services.FileReaders;

namespace DeepSeekSurveyAnalyzer.Services;

public class FileReaderFactory
{
    private readonly IEnumerable<IFileReader> _readers;

    public FileReaderFactory()
    {
        _readers = new List<IFileReader>
        {
            new PdfFileReader(),
            new DocxFileReader(),
            new ExcelFileReader()
        };
    }

    public IFileReader? GetReader(string filePath)
    {
        return _readers.FirstOrDefault(r => r.CanRead(filePath));
    }
}