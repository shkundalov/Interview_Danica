using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InterviewDanica.Api.Controllers;
/*
* Communication controller
* request header: Idempotency-Key (optional) - if presented, the same request will return the cached response
* request body: { customerId: UUID, templateId: UUID }
* Actions: get customed DTO, get template DTO, build template, sending email, return response
*/
//[Authorize] - Commented for Swagger tests
[ApiController]
[Route("api/[controller]")]
public class CommunicationsController : ControllerBase {
    private readonly ICommunicationService communicationService;
    private readonly IIdempotencyService idempotencyService;
    private readonly ILogger<CommunicationsController> log;

    public CommunicationsController(ICommunicationService communicationService,IIdempotencyService idempotencyService,ILogger<CommunicationsController> logger) {
        this.communicationService = communicationService;
        this.idempotencyService = idempotencyService;
        log = logger;
    }
    [HttpPost("send")]
    [ProducesResponseType(typeof(SendCommunicationResponseDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Send([FromBody] SendCommunicationRequestDTO request,[FromHeader(Name = "Idempotency-Key")] string? idempotencyKey) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }
        if (!string.IsNullOrWhiteSpace(idempotencyKey)) {
            var response = await idempotencyService.CheckCache(idempotencyKey);
            if (response.WasProcessed && response.CachedResponse != null) {
                var cachedResult = JsonSerializer.Deserialize<SendCommunicationResponseDTO>(response.CachedResponse);
                return Ok(cachedResult);
            }
        }
        try {
            var result = await communicationService.SendCommunication(request.CustomerId,request.TemplateId);
            if (!result.Success) {
                return NotFound(new { message = result.Message });
            }
            if (!string.IsNullOrWhiteSpace(idempotencyKey)) {
                var responseJson = JsonSerializer.Serialize(result);
                await idempotencyService.CacheResponse(
                    idempotencyKey,
                    JsonSerializer.Serialize(request),
                    responseJson,
                    StatusCodes.Status200OK);
            }
            return Ok(result);
        }
        catch (Exception ex) {
            log.LogError("Unexpected error sending communication: {Exception}",ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error" });
        }
    }
}
