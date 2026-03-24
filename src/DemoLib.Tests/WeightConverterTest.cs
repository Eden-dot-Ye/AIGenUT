using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class WeightConverterTest
{
	[Test]
	public void TestKgToLbs_WhenPositiveValue_ThenConvertsCorrectly()
	{
		var result = WeightConverter.KgToLbs(1m);
		Assert.That(result, Is.EqualTo(2.2046m));
	}

	[Test]
	public void TestKgToLbs_WhenZero_ThenReturnsZero()
	{
		var result = WeightConverter.KgToLbs(0m);
		Assert.That(result, Is.EqualTo(0m));
	}

	[Test]
	public void TestKgToLbs_WhenNegative_ThenThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => WeightConverter.KgToLbs(-1m));
	}

	[Test]
	public void TestLbsToKg_WhenPositiveValue_ThenConvertsCorrectly()
	{
		var result = WeightConverter.LbsToKg(1m);
		Assert.That(result, Is.EqualTo(0.4536m));
	}

	[Test]
	public void TestLbsToKg_WhenZero_ThenReturnsZero()
	{
		var result = WeightConverter.LbsToKg(0m);
		Assert.That(result, Is.EqualTo(0m));
	}

	[Test]
	public void TestLbsToKg_WhenNegative_ThenThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => WeightConverter.LbsToKg(-5m));
	}

	[Test]
	public void TestKgToOz_WhenPositiveValue_ThenConvertsCorrectly()
	{
		var result = WeightConverter.KgToOz(1m);
		Assert.That(result, Is.EqualTo(35.274m));
	}

	[Test]
	public void TestKgToOz_WhenZero_ThenReturnsZero()
	{
		var result = WeightConverter.KgToOz(0m);
		Assert.That(result, Is.EqualTo(0m));
	}

	[Test]
	public void TestKgToOz_WhenNegative_ThenThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => WeightConverter.KgToOz(-1m));
	}

	[Test]
	public void TestTonneToKg_WhenPositiveValue_ThenConvertsCorrectly()
	{
		var result = WeightConverter.TonneToKg(2.5m);
		Assert.That(result, Is.EqualTo(2500m));
	}

	[Test]
	public void TestTonneToKg_WhenZero_ThenReturnsZero()
	{
		var result = WeightConverter.TonneToKg(0m);
		Assert.That(result, Is.EqualTo(0m));
	}

	[Test]
	public void TestTonneToKg_WhenNegative_ThenThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => WeightConverter.TonneToKg(-1m));
	}

	[Test]
	public void TestCalculateVolumetricWeight_WhenValidDimensions_ThenCalculatesCorrectly()
	{
		// 50 * 40 * 30 / 5000 = 12.00
		var result = WeightConverter.CalculateVolumetricWeight(50m, 40m, 30m);
		Assert.That(result, Is.EqualTo(12.00m));
	}

	[Test]
	public void TestCalculateVolumetricWeight_WhenCustomDivisor_ThenUsesIt()
	{
		// 60 * 40 * 30 / 6000 = 12.00
		var result = WeightConverter.CalculateVolumetricWeight(60m, 40m, 30m, 6000);
		Assert.That(result, Is.EqualTo(12.00m));
	}

	[Test]
	public void TestCalculateVolumetricWeight_WhenNegativeDimension_ThenThrows()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			WeightConverter.CalculateVolumetricWeight(-1m, 40m, 30m));
	}

	[Test]
	public void TestCalculateVolumetricWeight_WhenZeroDivisor_ThenThrows()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			WeightConverter.CalculateVolumetricWeight(50m, 40m, 30m, 0));
	}

	[Test]
	public void TestKgToLbs_WhenLargeValue_ThenPrecisionMaintained()
	{
		var result = WeightConverter.KgToLbs(1000m);
		Assert.That(result, Is.EqualTo(2204.62m));
	}
}
