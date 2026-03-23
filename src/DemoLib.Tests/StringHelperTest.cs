using NUnit.Framework;

namespace DemoLib.Tests;

/// <summary>
/// Unit tests for StringHelper — demonstrates the target test style.
/// This file shows what AI-generated tests should look like.
/// </summary>
[TestFixture]
public class StringHelperTest
{
	[Test]
	public void TestTruncate_WhenStringLongerThanMax_ThenTruncated()
	{
		var result = StringHelper.Truncate("Hello, World!", 5);
		Assert.That(result, Is.EqualTo("Hello"));
	}

	[Test]
	public void TestTruncate_WhenStringShorterThanMax_ThenUnchanged()
	{
		var result = StringHelper.Truncate("Hi", 10);
		Assert.That(result, Is.EqualTo("Hi"));
	}

	[Test]
	public void TestTruncate_WhenMaxLengthIsZero_ThenReturnsEmpty()
	{
		var result = StringHelper.Truncate("Hello", 0);
		Assert.That(result, Is.EqualTo(""));
	}

	[Test]
	public void TestTruncate_WhenNullInput_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => StringHelper.Truncate(null!, 5));
	}

	[Test]
	public void TestTruncate_WhenNegativeMaxLength_ThenThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => StringHelper.Truncate("Hello", -1));
	}

	[Test]
	public void TestIsAlphanumericAndSpaces_WhenValid_ThenReturnsTrue()
	{
		Assert.That(StringHelper.IsAlphanumericAndSpaces("ABC 123"), Is.True);
	}

	[Test]
	public void TestIsAlphanumericAndSpaces_WhenContainsSpecialChars_ThenReturnsFalse()
	{
		Assert.That(StringHelper.IsAlphanumericAndSpaces("ABC-123!"), Is.False);
	}

	[Test]
	public void TestIsAlphanumericAndSpaces_WhenEmpty_ThenReturnsTrue()
	{
		Assert.That(StringHelper.IsAlphanumericAndSpaces(""), Is.True);
	}

	[Test]
	public void TestToUrlSafeBase64_WhenStandardBase64_ThenConverted()
	{
		// "Hello" in standard Base64 = "SGVsbG8="
		var result = StringHelper.ToUrlSafeBase64("SGVsbG8=");
		Assert.That(result, Is.EqualTo("SGVsbG8"));
	}

	[Test]
	public void TestToUrlSafeBase64_WhenContainsPlusAndSlash_ThenReplaced()
	{
		var result = StringHelper.ToUrlSafeBase64("a+b/c==");
		Assert.That(result, Is.EqualTo("a-b_c"));
	}

	[Test]
	public void TestFromUrlSafeBase64_WhenUrlSafe_ThenRestoredToStandard()
	{
		// "a-b_c" → replace chars → "a+b/c" (length 5, 5%4=1 → no padding added)
		var result = StringHelper.FromUrlSafeBase64("a-b_c");
		Assert.That(result, Is.EqualTo("a+b/c"));
	}

	[Test]
	public void TestFromUrlSafeBase64_WhenPaddingNeeded2_ThenAddsTwoEquals()
	{
		// "ab" → length 2, 2%4=2 → add "=="
		var result = StringHelper.FromUrlSafeBase64("ab");
		Assert.That(result, Is.EqualTo("ab=="));
	}

	[Test]
	public void TestFromUrlSafeBase64_WhenPaddingNeeded1_ThenAddsOneEquals()
	{
		// "abc" → length 3, 3%4=3 → add "="
		var result = StringHelper.FromUrlSafeBase64("abc");
		Assert.That(result, Is.EqualTo("abc="));
	}

	[Test]
	public void TestBase64RoundTrip_WhenConvertedBothWays_ThenMatchesOriginal()
	{
		var original = "SGVsbG8gV29ybGQ=";
		var urlSafe = StringHelper.ToUrlSafeBase64(original);
		var restored = StringHelper.FromUrlSafeBase64(urlSafe);
		Assert.That(restored, Is.EqualTo(original));
	}
}
