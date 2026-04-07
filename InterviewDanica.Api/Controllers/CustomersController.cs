using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewDanica.Api.Controllers;

//[Authorize] - Commented for Swagger tests
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase {
    private readonly ICustomerService customerService;
    private readonly ILogger<CustomersController> log;

    public CustomersController( ICustomerService customerService, ILogger<CustomersController> logger) {
        this.customerService = customerService;
        log = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerResponseDTO>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() {
        var customers = await customerService.GetAllCustomers();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerResponseDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) {
        var customer = await customerService.GetCustomerById(id);
        if (customer == null)
            return NotFound(new { message = $"Customer with ID '{id}' not found" });
        return Ok(customer);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponseDTO),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequestDTO request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try {
            var customer = await customerService.CreateOrGetCustomer(request);
            return Ok(customer);
        }
        catch (InvalidOperationException ex) {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) {
            log.LogError("Unexpected error creating customer: {Exception}",ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateCustomerRequestDTO request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try {
            var success = await customerService.UpdateCustomer(id,request);
            if (!success)
                return NotFound(new { message = $"Customer with ID '{id}' not found" });
            return NoContent();
        }
        catch (InvalidOperationException ex) {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) {
            log.LogError("Unexpected error updating customer: {Exception}",ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id) {
        await customerService.DeleteCustomer(id);
        return Ok(new { message = "Success" });
    }
}
