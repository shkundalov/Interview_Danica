namespace InterviewDanica.Api.DTOs;
public class SendCommunicationRequestDTO {
    public Guid CustomerId { get; set; }
    public Guid TemplateId { get; set; }
}
public class SendCommunicationResponseDTO {
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
public class CacheResponse {
    public bool WasProcessed { get; set; } = false;
    public string? CachedResponse { get; set; }
}