using InterviewDanica.Api.Data;
using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Models;
using InterviewDanica.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace InterviewDanica.Api.Tests;

public class CustomerServiceTests {
    private readonly ITestOutputHelper xConsole;
    private readonly ILogger<CustomerService> log;
    private readonly DBContext db;
    public CustomerServiceTests(ITestOutputHelper xConsole) {
        this.xConsole = xConsole;
        this.log = Utils.GetMockLog<CustomerService>();
        this.db = Utils.GetMockDB();
    }
    [Fact]
    public async Task Customer_create_valid() {
        var customer = new CustomerService(db,log);
        string uuser = Utils.UString();
        var request = new CreateCustomerRequestDTO { Name = uuser,Email = $"{uuser}@null.net" };
        var response = await customer.CreateOrGetCustomer(request);
        Assert.NotNull(response);
        Assert.Equal(uuser,response.Name);
        Assert.Equal($"{uuser}@null.net",response.Email);
        xConsole.WriteLine("Customer_create_valid: PASS");
    }
    [Fact]
    public async Task Customer_getAll() {
        var customer = new CustomerService(db,log);
        string uuser = Utils.UString();
        db.Customers.Add(new Customer { Id = Guid.NewGuid(),Name = uuser,Email = $"{uuser}@null.net" });
        db.Customers.Add(new Customer { Id = Guid.NewGuid(),Name = $"{uuser}2",Email = $"{uuser}2@null.net" });
        await db.SaveChangesAsync();
        var response = await customer.GetAllCustomers();
        Assert.Equal(2,response.Count());
        xConsole.WriteLine("Customer_getAll: PASS");
    }
    [Fact]
    public async Task Customer_getById() {
        var customer = new CustomerService(db,log);
        var customerId = Guid.NewGuid();
        string uuser = Utils.UString();
        db.Customers.Add(new Customer { Id = customerId,Name = uuser,Email = $"{uuser}@null.net" });
        await db.SaveChangesAsync();
        var response = await customer.GetCustomerById(customerId);
        Assert.NotNull(response);
        Assert.Equal(uuser,response.Name);
        xConsole.WriteLine("Customer_getById: PASS");
    }
    [Fact]
    public async Task Castomer_update() {
        var customer = new CustomerService(db,log);
        var customerId = Guid.NewGuid();
        string uuser = Utils.UString();
        string uuser2 = Utils.UString();
        db.Customers.Add(new Customer { Id = customerId,Name = uuser,Email = $"{uuser}@null.net" });
        await db.SaveChangesAsync();
        var request = new UpdateCustomerRequestDTO { Name = uuser2,Email = $"{uuser2}@null.net" };
        var response = await customer.UpdateCustomer(customerId,request);
        Assert.True(response);
        var updated = await db.Customers.FindAsync(customerId);
        Assert.Equal(uuser2,updated!.Name);
        xConsole.WriteLine("Castomer_update: PASS");
    }
    [Fact]
    public async Task Customer_delete() {
        var customer = new CustomerService(db,log);
        var customerId = Guid.NewGuid();
        string uuser = Utils.UString();
        db.Customers.Add(new Customer { Id = customerId,Name = uuser,Email = $"{uuser}@null.net" });
        await db.SaveChangesAsync();
        var response = await customer.DeleteCustomer(customerId);
        Assert.True(response);
        var deleted = await db.Customers.FindAsync(customerId);
        Assert.Null(deleted);
        xConsole.WriteLine("Customer_delete: PASS");
    }
}
