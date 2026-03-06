using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.Services;

public class AnalysisService
{
    private readonly List<AnalysisResult> _results = new();

    public void SaveAnalysis(AnalysisResult result)
    {
        _results.Add(result);
    }

    public AnalysisResult? GetLatestAnalysis()
    {
        return _results.Count > 0 ? _results[^1] : null;
    }

    public List<AnalysisResult> GetAllAnalyses()
    {
        return new List<AnalysisResult>(_results);
    }

    public void ClearAnalyses()
    {
        _results.Clear();
    }
}