using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void TestConstructor_WhenZeroMaxTokens_ThenThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimiter(0, 1.0));
	}

	[Test]
	public void TestConstructor_WhenNegativeRefillRate_ThenThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimiter(10, -1.0));
	}

	[Test]
	public void TestTryAcquire_WhenTokensAvailable_ThenReturnsTrue()
	{
		var limiter = new RateLimiter(10, 1.0);
		Assert.That(limiter.TryAcquire(), Is.True);
	}

	[Test]
	public void TestTryAcquire_WhenAllTokensConsumed_ThenReturnsFalse()
	{
		var limiter = new RateLimiter(2, 0.001);
		limiter.TryAcquire();
		limiter.TryAcquire();
		Assert.That(limiter.TryAcquire(), Is.False);
	}

	[Test]
	public void TestTryAcquire_WhenMultipleTokensRequested_ThenConsumesMultiple()
	{
		var limiter = new RateLimiter(10, 1.0);
		Assert.That(limiter.TryAcquire(5), Is.True);
		Assert.That(limiter.TryAcquire(6), Is.False);
	}

	[Test]
	public void TestTryAcquire_WhenRequestExceedsMax_ThenReturnsFalse()
	{
		var limiter = new RateLimiter(5, 1.0);
		Assert.That(limiter.TryAcquire(6), Is.False);
	}

	[Test]
	public void TestTryAcquire_WhenZeroTokensRequested_ThenThrows()
	{
		var limiter = new RateLimiter(10, 1.0);
		Assert.Throws<ArgumentOutOfRangeException>(() => limiter.TryAcquire(0));
	}

	[Test]
	public void TestAvailableTokens_WhenSomeConsumed_ThenReflectsRemaining()
	{
		var limiter = new RateLimiter(10, 1.0);
		limiter.TryAcquire(3);
		// Available tokens should be around 7 (plus any tiny refill)
		Assert.That(limiter.AvailableTokens, Is.LessThanOrEqualTo(10.0));
		Assert.That(limiter.AvailableTokens, Is.GreaterThanOrEqualTo(7.0));
	}

	[Test]
	public void TestGetWaitTime_WhenTokensNeeded_ThenReturnsPositiveWait()
	{
		var limiter = new RateLimiter(5, 1.0);
		limiter.TryAcquire(5);
		var waitTime = limiter.GetWaitTime(3);
		Assert.That(waitTime, Is.GreaterThanOrEqualTo(0.0));
	}

	[Test]
	public void TestReset_WhenCalled_ThenTokensRestored()
	{
		var limiter = new RateLimiter(10, 1.0);
		limiter.TryAcquire(10);
		limiter.Reset();
		Assert.That(limiter.TryAcquire(10), Is.True);
	}

	[Test]
	public void TestGetWaitTime_WhenTokensAvailable_ThenReturnsZero()
	{
		var limiter = new RateLimiter(10, 1.0);
		Assert.That(limiter.GetWaitTime(1), Is.EqualTo(0.0));
	}

	[Test]
	public void TestGetWaitTime_WhenTokensExceedMax_ThenReturnsInfinity()
	{
		var limiter = new RateLimiter(5, 1.0);
		Assert.That(limiter.GetWaitTime(6), Is.EqualTo(double.PositiveInfinity));
	}

	[Test]
	public void TestMaxTokens_WhenCreated_ThenReturnsConfiguredValue()
	{
		var limiter = new RateLimiter(42, 1.0);
		Assert.That(limiter.MaxTokens, Is.EqualTo(42));
	}

	[Test]
	public void TestRefillRate_WhenCreated_ThenReturnsConfiguredValue()
	{
		var limiter = new RateLimiter(10, 3.5);
		Assert.That(limiter.RefillRate, Is.EqualTo(3.5));
	}
}

[TestFixture]
public class SlidingWindowRateLimiterTest
{
	[Test]
	public void TestConstructor_WhenZeroMaxRequests_ThenThrows()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new SlidingWindowRateLimiter(0, TimeSpan.FromSeconds(1)));
	}

	[Test]
	public void TestConstructor_WhenZeroWindow_ThenThrows()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new SlidingWindowRateLimiter(10, TimeSpan.Zero));
	}

	[Test]
	public void TestTryRecord_WhenUnderLimit_ThenReturnsTrue()
	{
		var limiter = new SlidingWindowRateLimiter(5, TimeSpan.FromMinutes(1));
		Assert.That(limiter.TryRecord(), Is.True);
	}

	[Test]
	public void TestTryRecord_WhenAtLimit_ThenReturnsFalse()
	{
		var limiter = new SlidingWindowRateLimiter(2, TimeSpan.FromMinutes(1));
		limiter.TryRecord();
		limiter.TryRecord();
		Assert.That(limiter.TryRecord(), Is.False);
	}

	[Test]
	public void TestRemainingRequests_WhenSomeUsed_ThenReturnsRemaining()
	{
		var limiter = new SlidingWindowRateLimiter(5, TimeSpan.FromMinutes(1));
		limiter.TryRecord();
		limiter.TryRecord();
		Assert.That(limiter.RemainingRequests, Is.EqualTo(3));
	}

	[Test]
	public void TestReset_WhenCalled_ThenAllRequestsAvailable()
	{
		var limiter = new SlidingWindowRateLimiter(3, TimeSpan.FromMinutes(1));
		limiter.TryRecord();
		limiter.TryRecord();
		limiter.TryRecord();
		limiter.Reset();
		Assert.That(limiter.RemainingRequests, Is.EqualTo(3));
	}

	[Test]
	public void TestMaxRequests_WhenCreated_ThenReturnsConfiguredValue()
	{
		var limiter = new SlidingWindowRateLimiter(10, TimeSpan.FromSeconds(30));
		Assert.That(limiter.MaxRequests, Is.EqualTo(10));
	}

	[Test]
	public void TestWindow_WhenCreated_ThenReturnsConfiguredValue()
	{
		var window = TimeSpan.FromSeconds(30);
		var limiter = new SlidingWindowRateLimiter(10, window);
		Assert.That(limiter.Window, Is.EqualTo(window));
	}
}
