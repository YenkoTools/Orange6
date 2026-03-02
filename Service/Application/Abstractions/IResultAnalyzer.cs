namespace Application.Abstractions;

/// <summary>
/// Analyzes a query result and extracts structured information about it.
/// </summary>
public interface IResultAnalyzer
{
    ResultAnalysis AnalyzeResult(object? result);
}

/// <summary>
/// The result of analyzing a query result object.
/// </summary>
public record ResultAnalysis(bool IsSuccess, string ResultType, int? ItemCount);
