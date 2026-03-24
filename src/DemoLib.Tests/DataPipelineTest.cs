using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class DataPipelineTest
{
	[Test]
	public void TestWhere_WhenFilterApplied_ThenOnlyMatchingElements()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline.Where(x => x > 3).ExecuteToList(new[] { 1, 2, 3, 4, 5 });
		Assert.That(result, Is.EqualTo(new[] { 4, 5 }));
	}

	[Test]
	public void TestOrderBy_WhenApplied_ThenElementsSorted()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline.OrderBy(x => x).ExecuteToList(new[] { 3, 1, 2 });
		Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
	}

	[Test]
	public void TestOrderByDescending_WhenApplied_ThenElementsSortedDescending()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline.OrderByDescending(x => x).ExecuteToList(new[] { 1, 3, 2 });
		Assert.That(result, Is.EqualTo(new[] { 3, 2, 1 }));
	}

	[Test]
	public void TestDistinct_WhenDuplicatesExist_ThenRemoved()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline.Distinct().ExecuteToList(new[] { 1, 2, 2, 3, 3 });
		Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
	}

	[Test]
	public void TestTake_WhenApplied_ThenLimitsElements()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline.Take(2).ExecuteToList(new[] { 1, 2, 3, 4 });
		Assert.That(result, Is.EqualTo(new[] { 1, 2 }));
	}

	[Test]
	public void TestSkip_WhenApplied_ThenSkipsElements()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline.Skip(2).ExecuteToList(new[] { 1, 2, 3, 4 });
		Assert.That(result, Is.EqualTo(new[] { 3, 4 }));
	}

	[Test]
	public void TestChaining_WhenMultipleSteps_ThenAllApplied()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline
			.Where(x => x > 1)
			.OrderByDescending(x => x)
			.Take(2)
			.ExecuteToList(new[] { 5, 1, 3, 2, 4 });
		Assert.That(result, Is.EqualTo(new[] { 5, 4 }));
	}

	[Test]
	public void TestExecuteToList_WhenNoSteps_ThenReturnsOriginal()
	{
		var pipeline = new DataPipeline<int>();
		var result = pipeline.ExecuteToList(new[] { 1, 2, 3 });
		Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
	}

	[Test]
	public void TestExecuteCount_WhenFiltered_ThenReturnsFilteredCount()
	{
		var pipeline = new DataPipeline<int>();
		var count = pipeline.Where(x => x % 2 == 0).ExecuteCount(new[] { 1, 2, 3, 4, 5 });
		Assert.That(count, Is.EqualTo(2));
	}

	[Test]
	public void TestReset_WhenCalled_ThenPipelineCleared()
	{
		var pipeline = new DataPipeline<int>();
		pipeline.Where(x => x > 10);
		pipeline.Reset();
		Assert.That(pipeline.Steps, Is.Empty);
		var result = pipeline.ExecuteToList(new[] { 1, 2, 3 });
		Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
	}

	[Test]
	public void TestTake_WhenNegativeCount_ThenThrows()
	{
		var pipeline = new DataPipeline<int>();
		Assert.Throws<ArgumentOutOfRangeException>(() => pipeline.Take(-1));
	}

	[Test]
	public void TestSkip_WhenNegativeCount_ThenThrows()
	{
		var pipeline = new DataPipeline<int>();
		Assert.Throws<ArgumentOutOfRangeException>(() => pipeline.Skip(-1));
	}

	[Test]
	public void TestPipelineWithSelect_WhenProjection_ThenTransformsElements()
	{
		var pipeline = new DataPipeline<int, string>();
		var result = pipeline
			.Where(x => x > 1)
			.Select(x => x.ToString())
			.ExecuteToList(new[] { 1, 2, 3 });
		Assert.That(result, Is.EqualTo(new[] { "2", "3" }));
	}

	[Test]
	public void TestPipelineWithSelect_WhenNoProjection_ThenThrowsInvalidOperationException()
	{
		var pipeline = new DataPipeline<int, string>();
		Assert.Throws<InvalidOperationException>(() =>
			pipeline.ExecuteToList(new[] { 1, 2, 3 }));
	}

	[Test]
	public void TestCountBy_WhenGrouped_ThenCountsPerGroup()
	{
		var data = new[] { "apple", "banana", "apple", "cherry", "banana", "apple" };
		var counts = DataAggregator.CountBy(data, x => x);
		Assert.That(counts["apple"], Is.EqualTo(3));
		Assert.That(counts["banana"], Is.EqualTo(2));
		Assert.That(counts["cherry"], Is.EqualTo(1));
	}

	[Test]
	public void TestSumBy_WhenGrouped_ThenSumsPerGroup()
	{
		var data = new[] { ("A", 10m), ("B", 20m), ("A", 30m) };
		var sums = DataAggregator.SumBy(data, x => x.Item1, x => x.Item2);
		Assert.That(sums["A"], Is.EqualTo(40m));
		Assert.That(sums["B"], Is.EqualTo(20m));
	}

	[Test]
	public void TestRunningTotal_WhenMultipleValues_ThenCumulativeSums()
	{
		var result = DataAggregator.RunningTotal(new[] { 1m, 2m, 3m, 4m }).ToList();
		Assert.That(result, Is.EqualTo(new[] { 1m, 3m, 6m, 10m }));
	}

	[Test]
	public void TestRunningTotal_WhenEmpty_ThenEmptyResult()
	{
		var result = DataAggregator.RunningTotal(Array.Empty<decimal>()).ToList();
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void TestComputeStatistics_WhenValidData_ThenCorrectStats()
	{
		var stats = DataAggregator.ComputeStatistics(new[] { 2.0, 4.0, 6.0, 8.0, 10.0 });
		Assert.That(stats.Count, Is.EqualTo(5));
		Assert.That(stats.Sum, Is.EqualTo(30.0));
		Assert.That(stats.Mean, Is.EqualTo(6.0));
		Assert.That(stats.Median, Is.EqualTo(6.0));
		Assert.That(stats.Min, Is.EqualTo(2.0));
		Assert.That(stats.Max, Is.EqualTo(10.0));
	}

	[Test]
	public void TestComputeStatistics_WhenEmptyData_ThenThrowsInvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() =>
			DataAggregator.ComputeStatistics(Array.Empty<double>()));
	}
}
