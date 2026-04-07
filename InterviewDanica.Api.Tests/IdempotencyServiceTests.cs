using InterviewDanica.Api.Data;
using InterviewDanica.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace InterviewDanica.Api.Tests;

public class IdempotencyServiceTests {
    private readonly ITestOutputHelper xConsole;
    private readonly ILogger<IdempotencyService> log;
    private readonly DBContext db;
    public IdempotencyServiceTests(ITestOutputHelper xConsole) {
        this.xConsole = xConsole;
        this.log = Utils.GetMockLog<IdempotencyService>();
        this.db = Utils.GetMockDB();
    }
    [Fact]
    public async Task Idempotency_NoCached() {
        var idempotancy = new IdempotencyService(db,log);
        var idempotencyKey = Guid.NewGuid().ToString();
        var response = await idempotancy.CheckCache(idempotencyKey);
        Assert.False(response.WasProcessed);
        Assert.Null(response.CachedResponse);
        xConsole.WriteLine("Idempotency_NoCached: PASS");
    }
    [Fact]
    public async Task Idempotency_Cached_Found() {
        var idempotancy = new IdempotencyService(db,log);
        var idempotencyKey = Guid.NewGuid().ToString();
        var cachedJson = """{"id": "123", "name": "string"}""";
        await idempotancy.CacheResponse(idempotencyKey,"string",cachedJson,201);
        var response = await idempotancy.CheckCache(idempotencyKey);
        Assert.True(response.WasProcessed);
        Assert.Equal(cachedJson,response.CachedResponse);
        xConsole.WriteLine("Idempotency_Cached_Found: PASS");
    }
    [Fact]
    public async Task Idempotency_Error_noThrow() {
        var logger = log;
        var idempotancy = new IdempotencyService(db,logger);
        var idempotencyKey = Guid.NewGuid().ToString();
        await idempotancy.CacheResponse(idempotencyKey,"req","resp",200);
        await idempotancy.CacheResponse(idempotencyKey,"req","resp",200);
        // Should not throw an error
        xConsole.WriteLine("Idempotency_Error_noThrow: PASS");
    }
}
