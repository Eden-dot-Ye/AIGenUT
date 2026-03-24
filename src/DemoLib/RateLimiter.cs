namespace DemoLib;

/// <summary>
/// Implements a token bucket rate limiter for controlling access rates.
/// Thread-safe implementation suitable for concurrent environments.
/// </summary>
public class RateLimiter
{
    private readonly object _lock = new();
    private readonly int _maxTokens;
    private readonly double _refillRate; // tokens per second
    private double _currentTokens;
    private DateTime _lastRefill;

    /// <summary>
    /// Gets the maximum number of tokens (burst capacity).
    /// </summary>
    public int MaxTokens => _maxTokens;

    /// <summary>
    /// Gets the refill rate in tokens per second.
    /// </summary>
    public double RefillRate => _refillRate;

    /// <summary>
    /// Gets the current number of available tokens.
    /// </summary>
    public double AvailableTokens
    {
        get
        {
            lock (_lock)
            {
                RefillTokens();
                return _currentTokens;
            }
        }
    }

    /// <summary>
    /// Creates a new rate limiter with the specified capacity and refill rate.
    /// </summary>
    /// <param name="maxTokens">Maximum token capacity (burst size).</param>
    /// <param name="refillRate">Tokens added per second.</param>
    public RateLimiter(int maxTokens, double refillRate)
    {
        if (maxTokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "Max tokens must be positive.");
        if (refillRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(refillRate), "Refill rate must be positive.");

        _maxTokens = maxTokens;
        _refillRate = refillRate;
        _currentTokens = maxTokens;
        _lastRefill = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a rate limiter with a custom time provider (for testing).
    /// </summary>
    internal RateLimiter(int maxTokens, double refillRate, DateTime initialTime)
    {
        if (maxTokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "Max tokens must be positive.");
        if (refillRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(refillRate), "Refill rate must be positive.");

        _maxTokens = maxTokens;
        _refillRate = refillRate;
        _currentTokens = maxTokens;
        _lastRefill = initialTime;
    }

    /// <summary>
    /// Attempts to acquire one token. Returns true if successful.
    /// </summary>
    public bool TryAcquire()
    {
        return TryAcquire(1);
    }

    /// <summary>
    /// Attempts to acquire the specified number of tokens. Returns true if successful.
    /// </summary>
    public bool TryAcquire(int tokens)
    {
        if (tokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(tokens), "Token count must be positive.");
        if (tokens > _maxTokens)
            return false;

        lock (_lock)
        {
            RefillTokens();
            if (_currentTokens >= tokens)
            {
                _currentTokens -= tokens;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Calculates the wait time in seconds until the specified tokens become available.
    /// Returns 0 if tokens are already available.
    /// </summary>
    public double GetWaitTime(int tokens = 1)
    {
        if (tokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(tokens), "Token count must be positive.");
        if (tokens > _maxTokens)
            return double.PositiveInfinity;

        lock (_lock)
        {
            RefillTokens();
            if (_currentTokens >= tokens)
                return 0;

            var deficit = tokens - _currentTokens;
            return deficit / _refillRate;
        }
    }

    /// <summary>
    /// Resets the rate limiter to full capacity.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _currentTokens = _maxTokens;
            _lastRefill = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Simulates a time advance for testing purposes.
    /// </summary>
    internal void SimulateTimeAdvance(double seconds)
    {
        if (seconds < 0)
            throw new ArgumentOutOfRangeException(nameof(seconds), "Cannot go back in time.");

        lock (_lock)
        {
            var tokensToAdd = seconds * _refillRate;
            _currentTokens = Math.Min(_maxTokens, _currentTokens + tokensToAdd);
        }
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastRefill).TotalSeconds;
        if (elapsed > 0)
        {
            var tokensToAdd = elapsed * _refillRate;
            _currentTokens = Math.Min(_maxTokens, _currentTokens + tokensToAdd);
            _lastRefill = now;
        }
    }
}

/// <summary>
/// A sliding window rate limiter that tracks requests within a time window.
/// </summary>
public class SlidingWindowRateLimiter
{
    private readonly object _lock = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    private readonly Queue<DateTime> _requestTimestamps = new();

    /// <summary>
    /// Maximum requests allowed within the window.
    /// </summary>
    public int MaxRequests => _maxRequests;

    /// <summary>
    /// The time window duration.
    /// </summary>
    public TimeSpan Window => _window;

    /// <summary>
    /// Current number of requests in the window.
    /// </summary>
    public int CurrentRequestCount
    {
        get
        {
            lock (_lock)
            {
                PurgeExpired();
                return _requestTimestamps.Count;
            }
        }
    }

    public SlidingWindowRateLimiter(int maxRequests, TimeSpan window)
    {
        if (maxRequests <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRequests), "Max requests must be positive.");
        if (window <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(window), "Window must be positive.");

        _maxRequests = maxRequests;
        _window = window;
    }

    /// <summary>
    /// Records a request and returns true if allowed, false if rate limited.
    /// </summary>
    public bool TryRecord()
    {
        lock (_lock)
        {
            PurgeExpired();
            if (_requestTimestamps.Count >= _maxRequests)
                return false;

            _requestTimestamps.Enqueue(DateTime.UtcNow);
            return true;
        }
    }

    /// <summary>
    /// Returns the remaining allowed requests in the current window.
    /// </summary>
    public int RemainingRequests
    {
        get
        {
            lock (_lock)
            {
                PurgeExpired();
                return Math.Max(0, _maxRequests - _requestTimestamps.Count);
            }
        }
    }

    /// <summary>
    /// Resets the rate limiter.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _requestTimestamps.Clear();
        }
    }

    private void PurgeExpired()
    {
        var cutoff = DateTime.UtcNow - _window;
        while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < cutoff)
            _requestTimestamps.Dequeue();
    }
}
