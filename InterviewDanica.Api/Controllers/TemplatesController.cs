using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewDanica.Api.Controllers;

//[Authorize] - Commented for Swagger tests
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase {
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(
        ITemplateService templateService,
        ILogger<TemplatesController> logger) {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TemplateResponseDTO>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() {
        var templates = await _templateService.GetAllTemplates();
        return Ok(templates);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TemplateResponseDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) {
        var template = await _templateService.GetTemplateById(id);
        if (template == null)
            return NotFound(new { message = $"Template with ID '{id}' not found" });
        return Ok(template);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TemplateResponseDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequestDTO request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try {
            var template = await _templateService.CreateOrGetTemplate(request);
            return Ok(template);
        }
        catch (InvalidOperationException ex) {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) {
            _logger.LogError("Unexpected error creating template: {Exception}",ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateTemplateRequestDTO request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try {
            var success = await _templateService.UpdateTemplate(id,request);
            if (!success)
                return NotFound(new { message = $"Template with ID '{id}' not found" });
            return NoContent();
        }
        catch (InvalidOperationException ex) {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) {
            _logger.LogError("Unexpected error updating template: {Exception}",ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id) {
        await _templateService.DeleteTemplate(id);
        return Ok(new { message = "Success" });
    }
}
