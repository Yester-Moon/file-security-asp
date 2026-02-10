using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace FileService.WebAPI.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _maxRequests = 100;
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<SkipRateLimitAttribute>() != null)
        {
            await _next(context);
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"rate_limit_{ipAddress}";
        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _timeWindow;
            return 0;
        });

        if (requestCount >= _maxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", ipAddress);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new
            {
                Message = "Too many requests. Please try again later.",
                RetryAfter = _timeWindow.TotalSeconds
            });
            return;
        }

        _cache.Set(cacheKey, requestCount + 1, _timeWindow);
        await _next(context);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipRateLimitAttribute : Attribute
{
}
