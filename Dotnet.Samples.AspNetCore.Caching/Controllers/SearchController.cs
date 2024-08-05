using Dotnet.Samples.AspNetCore.Caching.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnet.Samples.AspNetCore.Caching.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(IMemoryCache cache) : ControllerBase
{
    private readonly IMemoryCache _memoryCache = cache;

    [HttpGet("inmemorycache")]
    public async Task<IResult> SearchWithInMemoryCache([FromQuery] string q)
    {
        if (string.IsNullOrEmpty(q)) return TypedResults.BadRequest();

        var cacheKey = $"SearchResult_{q}";
        var cacheStatus = "Miss"; // Default to cache miss

        if (!_memoryCache.TryGetValue(cacheKey, out SearchResult? searchResult))
        {
            // Mock
            searchResult = new SearchResult()
            {
                Query = q,
                Content = "Lorem ipsum dolor sit amet.",
                Source = "Context"
            };

            await Task.CompletedTask;

            _memoryCache.Set(cacheKey, searchResult, GetMemoryCacheEntryOptions());

            return TypedResults.Ok(searchResult);
        }
        else
        {
            searchResult!.Source = "Cache";
            cacheStatus = "Hit";
        }

        Response.Headers["X-Cache-Status"] = cacheStatus;

        return TypedResults.Ok(searchResult);
    }

        [HttpGet("responsecaching")]
        // Duration = 86400 (1 day)
        // Location = ResponseCacheLocation.Any (cached in both proxies and browsers)
        // VaryByQueryKeys = ["q"] (the search term)
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["q"])]
        public async Task<IResult> SearchWithResponseCaching([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q)) return TypedResults.BadRequest();

            // Mock
            var searchResult = new SearchResult()
            {
                Query = q,
                Content = "Lorem ipsum dolor sit amet.",
                Source = "Context",
            };

            await Task.CompletedTask;

            return TypedResults.Ok(searchResult);
        }

    private static MemoryCacheEntryOptions GetMemoryCacheEntryOptions()
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8),
            SlidingExpiration = TimeSpan.FromHours(1)
        };
    }
}
