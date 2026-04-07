namespace InterviewDanica.Api.Models;
public class ProcessedRequest {
    public string IdempotencyKey { get; set; } = null!; //Server generated UUID
    public string RequestData { get; set; } = null!;
    public string ResponseData { get; set; } = null!;
    public int StatusCode { get; set; }
    public DateTime ProcessedAt { get; set; }
}
