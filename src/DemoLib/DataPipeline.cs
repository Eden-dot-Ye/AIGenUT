namespace DemoLib;

/// <summary>
/// Provides a fluent API for building and executing data transformation pipelines.
/// Supports mapping, filtering, sorting, grouping, aggregation, and chaining.
/// </summary>
public class DataPipeline<T>
{
    private readonly List<Func<IEnumerable<T>, IEnumerable<T>>> _steps = new();
    private readonly List<string> _stepDescriptions = new();

    /// <summary>
    /// Gets the descriptions of all steps in the pipeline.
    /// </summary>
    public IReadOnlyList<string> Steps => _stepDescriptions.AsReadOnly();

    /// <summary>
    /// Filters elements based on a predicate.
    /// </summary>
    public DataPipeline<T> Where(Func<T, bool> predicate, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _steps.Add(data => data.Where(predicate));
        _stepDescriptions.Add(description ?? "Filter");
        return this;
    }

    /// <summary>
    /// Orders elements by a key in ascending order.
    /// </summary>
    public DataPipeline<T> OrderBy<TKey>(Func<T, TKey> keySelector, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _steps.Add(data => data.OrderBy(keySelector));
        _stepDescriptions.Add(description ?? "OrderBy");
        return this;
    }

    /// <summary>
    /// Orders elements by a key in descending order.
    /// </summary>
    public DataPipeline<T> OrderByDescending<TKey>(Func<T, TKey> keySelector, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _steps.Add(data => data.OrderByDescending(keySelector));
        _stepDescriptions.Add(description ?? "OrderByDescending");
        return this;
    }

    /// <summary>
    /// Removes duplicate elements.
    /// </summary>
    public DataPipeline<T> Distinct(string? description = null)
    {
        _steps.Add(data => data.Distinct());
        _stepDescriptions.Add(description ?? "Distinct");
        return this;
    }

    /// <summary>
    /// Removes duplicate elements based on a key selector.
    /// </summary>
    public DataPipeline<T> DistinctBy<TKey>(Func<T, TKey> keySelector, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _steps.Add(data => data.GroupBy(keySelector).Select(g => g.First()));
        _stepDescriptions.Add(description ?? "DistinctBy");
        return this;
    }

    /// <summary>
    /// Takes only the first N elements.
    /// </summary>
    public DataPipeline<T> Take(int count, string? description = null)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
        _steps.Add(data => data.Take(count));
        _stepDescriptions.Add(description ?? $"Take({count})");
        return this;
    }

    /// <summary>
    /// Skips the first N elements.
    /// </summary>
    public DataPipeline<T> Skip(int count, string? description = null)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
        _steps.Add(data => data.Skip(count));
        _stepDescriptions.Add(description ?? $"Skip({count})");
        return this;
    }

    /// <summary>
    /// Executes the pipeline on the given data and returns the transformed result.
    /// </summary>
    public IEnumerable<T> Execute(IEnumerable<T> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        IEnumerable<T> result = data;
        foreach (var step in _steps)
        {
            result = step(result);
        }
        return result;
    }

    /// <summary>
    /// Executes the pipeline and returns the results as a list.
    /// </summary>
    public List<T> ExecuteToList(IEnumerable<T> data)
    {
        return Execute(data).ToList();
    }

    /// <summary>
    /// Executes the pipeline and returns the count of resulting elements.
    /// </summary>
    public int ExecuteCount(IEnumerable<T> data)
    {
        return Execute(data).Count();
    }

    /// <summary>
    /// Resets the pipeline by clearing all steps.
    /// </summary>
    public DataPipeline<T> Reset()
    {
        _steps.Clear();
        _stepDescriptions.Clear();
        return this;
    }
}

/// <summary>
/// A typed pipeline that adds projection (Select) capabilities, producing a different output type.
/// </summary>
public class DataPipeline<TIn, TOut>
{
    private readonly List<Func<IEnumerable<TIn>, IEnumerable<TIn>>> _preSteps = new();
    private Func<TIn, TOut>? _projection;
    private readonly List<Func<IEnumerable<TOut>, IEnumerable<TOut>>> _postSteps = new();

    /// <summary>
    /// Filters input elements.
    /// </summary>
    public DataPipeline<TIn, TOut> Where(Func<TIn, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _preSteps.Add(data => data.Where(predicate));
        return this;
    }

    /// <summary>
    /// Sets the projection function to transform elements.
    /// </summary>
    public DataPipeline<TIn, TOut> Select(Func<TIn, TOut> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _projection = selector;
        return this;
    }

    /// <summary>
    /// Orders output elements.
    /// </summary>
    public DataPipeline<TIn, TOut> OrderOutputBy<TKey>(Func<TOut, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        _postSteps.Add(data => data.OrderBy(keySelector));
        return this;
    }

    /// <summary>
    /// Takes only the first N output elements.
    /// </summary>
    public DataPipeline<TIn, TOut> TakeOutput(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        _postSteps.Add(data => data.Take(count));
        return this;
    }

    /// <summary>
    /// Executes the pipeline.
    /// </summary>
    public IEnumerable<TOut> Execute(IEnumerable<TIn> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (_projection == null)
            throw new InvalidOperationException("Projection (Select) must be defined before execution.");

        IEnumerable<TIn> input = data;
        foreach (var step in _preSteps)
            input = step(input);

        IEnumerable<TOut> output = input.Select(_projection);
        foreach (var step in _postSteps)
            output = step(output);

        return output;
    }

    /// <summary>
    /// Executes and returns results as a list.
    /// </summary>
    public List<TOut> ExecuteToList(IEnumerable<TIn> data)
    {
        return Execute(data).ToList();
    }
}

/// <summary>
/// Provides aggregate operations on data collections.
/// </summary>
public static class DataAggregator
{
    /// <summary>
    /// Groups data by a key and counts elements per group.
    /// </summary>
    public static Dictionary<TKey, int> CountBy<T, TKey>(
        IEnumerable<T> data, Func<T, TKey> keySelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(keySelector);
        return data.GroupBy(keySelector).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Groups data by a key and sums a numeric value per group.
    /// </summary>
    public static Dictionary<TKey, decimal> SumBy<T, TKey>(
        IEnumerable<T> data, Func<T, TKey> keySelector, Func<T, decimal> valueSelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(valueSelector);
        return data.GroupBy(keySelector).ToDictionary(g => g.Key, g => g.Sum(valueSelector));
    }

    /// <summary>
    /// Computes a running total (cumulative sum) over a sequence.
    /// </summary>
    public static IEnumerable<decimal> RunningTotal(IEnumerable<decimal> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        decimal sum = 0;
        foreach (var value in values)
        {
            sum += value;
            yield return sum;
        }
    }

    /// <summary>
    /// Computes basic statistics for a numeric sequence.
    /// </summary>
    public static DataStatistics ComputeStatistics(IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var list = values.ToList();
        if (list.Count == 0)
            throw new InvalidOperationException("Cannot compute statistics for an empty sequence.");

        var sorted = list.OrderBy(v => v).ToList();
        var count = sorted.Count;
        var sum = sorted.Sum();
        var mean = sum / count;
        var variance = sorted.Sum(v => (v - mean) * (v - mean)) / count;
        var stdDev = Math.Sqrt(variance);
        var median = count % 2 == 0
            ? (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0
            : sorted[count / 2];

        return new DataStatistics(count, sum, mean, median, sorted[0], sorted[^1], stdDev, variance);
    }
}

/// <summary>
/// Represents basic statistics for a numeric dataset.
/// </summary>
public record DataStatistics(
    int Count,
    double Sum,
    double Mean,
    double Median,
    double Min,
    double Max,
    double StandardDeviation,
    double Variance);
