using InterviewDanica.Api.Data;
using InterviewDanica.Api.Models;
using InterviewDanica.Api.DTOs;

namespace InterviewDanica.Api.Services;
/*
 * Used in commutication for email sending
 */
public interface IIdempotencyService {
    Task<CacheResponse> CheckCache(string iKey);
    Task CacheResponse(string iKey,string requestData,string responseData,int statusCode);
}

public class IdempotencyService : IIdempotencyService {
    private readonly DBContext db;
    private readonly ILogger<IdempotencyService> log;

    public IdempotencyService(DBContext context,ILogger<IdempotencyService> logger) {
        db = context;
        log = logger;
    }
    public async Task<CacheResponse> CheckCache(string iKey) {
        if (string.IsNullOrWhiteSpace(iKey)) {
            return new CacheResponse { WasProcessed = false,CachedResponse = null };
        }
        log.LogInformation("REQ: IND Checking {ikey}",iKey);
        var record = await db.ProcessedRequests.FindAsync(iKey);
        if (record != null) {
            log.LogWarning("RES: IND Checking {ikey} - cached",iKey);
            return new CacheResponse { WasProcessed = true,CachedResponse = record.ResponseData };
        }
        log.LogInformation("RES: IND Checking {ikey} - new",iKey);
        return new CacheResponse { WasProcessed = false,CachedResponse = null };
    }
    public async Task CacheResponse(string iKey,string requestData,string responseData,int statusCode) {
        if (string.IsNullOrWhiteSpace(iKey)) {
            return;
        }

        log.LogInformation("REQ: IND Cache {ikey}",iKey);
        var processedRequest = new ProcessedRequest {
            IdempotencyKey = iKey,
            RequestData = requestData,
            ResponseData = responseData,
            StatusCode = statusCode,
            ProcessedAt = DateTime.UtcNow // UTC for depracating old records, might be useful in future for cache clearance policies
        };
        
        try {
            db.ProcessedRequests.Add(processedRequest);
            await db.SaveChangesAsync();
            log.LogInformation("RES: IND Cache {ikey} - 200",iKey);
        }
        catch (Exception ex) {
            log.LogWarning("RES: IND Cache {ikey} - 500, {Exception}",iKey,ex.Message);
        }
    }
}
