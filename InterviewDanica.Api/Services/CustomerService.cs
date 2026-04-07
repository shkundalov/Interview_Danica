using InterviewDanica.Api.Data;
using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Models;
using InterviewDanica.Api.Validators;
using Microsoft.EntityFrameworkCore;

namespace InterviewDanica.Api.Services;
/*
 * In log an accurate states are recorded for reusability purpose in possible other services, yet Controller sets idempotant states
 */
public interface ICustomerService {
    Task<IEnumerable<CustomerResponseDTO>> GetAllCustomers();
    Task<CustomerResponseDTO?> GetCustomerById(Guid id);
    Task<CustomerResponseDTO?> _GetCustomerByEmail(string email); //Used internally, might be useful for other services in the future
    Task<CustomerResponseDTO> CreateOrGetCustomer(CreateCustomerRequestDTO request);
    Task<CustomerResponseDTO> _CreateCustomer(CreateCustomerRequestDTO request); //Same
    Task<bool> UpdateCustomer(Guid id,UpdateCustomerRequestDTO request);
    Task<bool> DeleteCustomer(Guid id);
}

public class CustomerService : ICustomerService {
    private readonly DBContext db = null!;
    private readonly ILogger<CustomerService> log = null!;
    private static CustomerResponseDTO BuildResponse(Customer record) {
        return new CustomerResponseDTO {
            Id = record.Id,
            Name = record.Name,
            Email = record.Email
        };
    }

    public CustomerService(DBContext context,ILogger<CustomerService> logger) {
        db = context;
        log = logger;
    }

    public async Task<IEnumerable<CustomerResponseDTO>> GetAllCustomers() {
        log.LogInformation("REQ: GET api/Customers");
        var records = await db.Customers.ToListAsync();
        var response = records.Select(BuildResponse);
        log.LogInformation("RES: GET api/Customers - 200, {count}",records.Count);
        return response;
    }
    public async Task<CustomerResponseDTO?> GetCustomerById(Guid id) {
        log.LogInformation("REQ: GET api/Customers/{id}",id);
        var record = await db.Customers.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null) {
            log.LogWarning("RES: GET api/Customers/{id} - 404",id);
            return null;
        }
        log.LogInformation("RES: GET api/Customers/{id} - 200, {name}",id,record.Name);
        return BuildResponse(record);
    }
    public async Task<CustomerResponseDTO?> _GetCustomerByEmail(string email) {
        log.LogInformation("REQ: GET​ api​/Customers/{email}",email);
        var record = await db.Customers.FirstOrDefaultAsync(r => r.Email == email);
        if (record == null) {
            log.LogInformation("RES: GET​ api​/Customers/{email} - 404",email);
            return null;
        }
        log.LogInformation("RES: GET​ api​/Customers/{email} - 200, {id}",email,record.Id);
        return BuildResponse(record);
    }
    public async Task<CustomerResponseDTO> CreateOrGetCustomer(CreateCustomerRequestDTO request) {
        if (!EmailValidator.IsValid(request.Email)) {
            log.LogInformation("REQ: POST api/Customers - 400, invalid email");
            throw new InvalidOperationException("Invalid email format");
        }
        var response = await this._GetCustomerByEmail(request.Email);
        if (response != null) {
            return response;
        }
        return await this._CreateCustomer(request);
    }
    public async Task<CustomerResponseDTO> _CreateCustomer(CreateCustomerRequestDTO request) {
        log.LogInformation("REQ: POST api/Customers {name}",request.Name);
        var record = new Customer {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email
        };
        db.Customers.Add(record);
        await db.SaveChangesAsync();
        log.LogInformation("RES: POST​ api​/Customers - 200, {id}",record.Id);
        return BuildResponse(record);
    }
    public async Task<bool> UpdateCustomer(Guid id,UpdateCustomerRequestDTO request) {
        if (!EmailValidator.IsValid(request.Email)) {
            log.LogInformation("REQ: PUT api​/Customers​ {id} - 400, invalid email",id);
            throw new InvalidOperationException("Invalid email format");
        }
        log.LogInformation("REQ: PUT api​/Customers​ {id}",id);
        var record = await db.Customers.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null) {
            log.LogWarning("RES: PUT api​/Customers​ {id} - 404",id);
            return false;
        }
        var emailTaken = await db.Customers.AnyAsync(r => r.Email == request.Email && r.Id != id);
        if (emailTaken) {
            log.LogWarning("RES: PUT api​/Customers​ {id} - 409, {email}",id,request.Email);
            throw new InvalidOperationException($"Email '{request.Email}' is already in use");
        }
        record.Name = request.Name;
        record.Email = request.Email;
        db.Customers.Update(record);
        await db.SaveChangesAsync();
        log.LogInformation("RES: PUT api​/Customers​ {id} - 200", id);
        return true;
    }
    public async Task<bool> DeleteCustomer(Guid id) {
        log.LogInformation("DELETE api​/Customers​ {id}",id);
        var record = await db.Customers.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null) {
            log.LogWarning("DELETE api​/Customers​ {id} - 404",id);
            return false;
        }
        db.Customers.Remove(record);
        await db.SaveChangesAsync();
        log.LogInformation("DELETE api​/Customers​ {id} - 200",id);
        return true;
    }
}
