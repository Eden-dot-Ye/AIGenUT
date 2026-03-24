using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class ShipmentValidatorTest
{
	[Test]
	public void TestIsValidContainerNumber_WhenValid_ThenReturnsTrue()
	{
		Assert.That(ShipmentValidator.IsValidContainerNumber("ABCU1234567"), Is.True);
	}

	[Test]
	public void TestIsValidContainerNumber_WhenLowercaseLetters_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidContainerNumber("abcu1234567"), Is.False);
	}

	[Test]
	public void TestIsValidContainerNumber_WhenTooFewDigits_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidContainerNumber("ABCU123456"), Is.False);
	}

	[Test]
	public void TestIsValidContainerNumber_WhenTooFewLetters_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidContainerNumber("ABC1234567"), Is.False);
	}

	[Test]
	public void TestIsValidContainerNumber_WhenNull_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidContainerNumber(null!), Is.False);
	}

	[Test]
	public void TestIsValidContainerNumber_WhenEmpty_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidContainerNumber(""), Is.False);
	}

	[Test]
	public void TestIsValidHsCode_WhenSixDigits_ThenReturnsTrue()
	{
		Assert.That(ShipmentValidator.IsValidHsCode("847130"), Is.True);
	}

	[Test]
	public void TestIsValidHsCode_WhenDotSeparated_ThenReturnsTrue()
	{
		Assert.That(ShipmentValidator.IsValidHsCode("84.71.30"), Is.True);
	}

	[Test]
	public void TestIsValidHsCode_WhenFullDotFormat_ThenReturnsTrue()
	{
		Assert.That(ShipmentValidator.IsValidHsCode("84.71.30.00"), Is.True);
	}

	[Test]
	public void TestIsValidHsCode_WhenTooShort_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidHsCode("8471"), Is.False);
	}

	[Test]
	public void TestIsValidHsCode_WhenNull_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidHsCode(null!), Is.False);
	}

	[Test]
	public void TestIsValidHsCode_WhenContainsLetters_ThenReturnsFalse()
	{
		Assert.That(ShipmentValidator.IsValidHsCode("84AB30"), Is.False);
	}

	[Test]
	public void TestCalculateTotalWeight_WhenValidWeights_ThenSumsCorrectly()
	{
		var weights = new List<decimal> { 10.5m, 20.3m, 5.0m };
		Assert.That(ShipmentValidator.CalculateTotalWeight(weights), Is.EqualTo(35.8m));
	}

	[Test]
	public void TestCalculateTotalWeight_WhenNull_ThenReturnsZero()
	{
		Assert.That(ShipmentValidator.CalculateTotalWeight(null), Is.EqualTo(0m));
	}

	[Test]
	public void TestCalculateTotalWeight_WhenEmpty_ThenReturnsZero()
	{
		Assert.That(ShipmentValidator.CalculateTotalWeight(new List<decimal>()), Is.EqualTo(0m));
	}

	[Test]
	public void TestCalculateTotalWeight_WhenNegativeWeights_ThenTreatsAsZero()
	{
		var weights = new List<decimal> { 10m, -5m, 20m };
		Assert.That(ShipmentValidator.CalculateTotalWeight(weights), Is.EqualTo(30m));
	}

	[Test]
	public void TestNormalizeTrackingReference_WhenMixedCase_ThenUppercase()
	{
		var result = ShipmentValidator.NormalizeTrackingReference("abc 123 def");
		Assert.That(result, Is.EqualTo("ABC123DEF"));
	}

	[Test]
	public void TestNormalizeTrackingReference_WhenLeadingTrailingSpaces_ThenTrimmed()
	{
		var result = ShipmentValidator.NormalizeTrackingReference("  TRACK123  ");
		Assert.That(result, Is.EqualTo("TRACK123"));
	}

	[Test]
	public void TestNormalizeTrackingReference_WhenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => ShipmentValidator.NormalizeTrackingReference(null!));
	}

	[Test]
	public void TestGetStatusDescription_WhenPND_ThenReturnsPending()
	{
		Assert.That(ShipmentValidator.GetStatusDescription("PND"), Is.EqualTo("Pending"));
	}

	[Test]
	public void TestGetStatusDescription_WhenINP_ThenReturnsInProgress()
	{
		Assert.That(ShipmentValidator.GetStatusDescription("INP"), Is.EqualTo("In Progress"));
	}

	[Test]
	public void TestGetStatusDescription_WhenCMP_ThenReturnsCompleted()
	{
		Assert.That(ShipmentValidator.GetStatusDescription("CMP"), Is.EqualTo("Completed"));
	}

	[Test]
	public void TestGetStatusDescription_WhenCAN_ThenReturnsCancelled()
	{
		Assert.That(ShipmentValidator.GetStatusDescription("CAN"), Is.EqualTo("Cancelled"));
	}

	[Test]
	public void TestGetStatusDescription_WhenHLD_ThenReturnsOnHold()
	{
		Assert.That(ShipmentValidator.GetStatusDescription("HLD"), Is.EqualTo("On Hold"));
	}

	[Test]
	public void TestGetStatusDescription_WhenUnknownCode_ThenReturnsUnknown()
	{
		Assert.That(ShipmentValidator.GetStatusDescription("XYZ"), Is.EqualTo("Unknown"));
	}

	[Test]
	public void TestGetStatusDescription_WhenNull_ThenReturnsUnknown()
	{
		Assert.That(ShipmentValidator.GetStatusDescription(null!), Is.EqualTo("Unknown"));
	}
}
