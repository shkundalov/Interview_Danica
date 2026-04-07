using InterviewDanica.Api.Models;
using InterviewDanica.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace InterviewDanica.Api.Tests;

public class CommunicationServiceTests {
    private readonly ITestOutputHelper xConsole;
    private readonly Data.DBContext db;
    private readonly ILogger<CustomerService> logCustomer;
    private readonly ILogger<TemplateService> logTemplate;
    private readonly ILogger<CommunicationService> logCommunication;
    public CommunicationServiceTests(ITestOutputHelper xConsole) {
        this.xConsole = xConsole;
        this.db = Utils.GetMockDB();
        this.logCustomer = Utils.GetMockLog<CustomerService>();
        this.logTemplate = Utils.GetMockLog<TemplateService>();
        this.logCommunication = Utils.GetMockLog<CommunicationService>();
    }
    [Fact]
    public async Task Communication_valid_inputs() {
        var customer = new CustomerService(db,logCustomer);
        var template = new TemplateService(db,logTemplate);
        var communication = new CommunicationService(customer,template,logCommunication);
        string uuser = Utils.UString();
        Guid customerId = Guid.NewGuid();
        Guid templateId = Guid.NewGuid();

        db.Customers.Add(new Customer { Id = customerId,Name = uuser,Email = $"{uuser}@null.net" });
        db.Templates.Add(new Template { Id = templateId,Name = "Template test",Subject = "Subject {{Name}}",Body = "Name {{Name}}, email {{Email}}" });
        await db.SaveChangesAsync();
        var response = await communication.SendCommunication(customerId,templateId);
        Assert.True(response.Success);
        Assert.Equal($"Subject {uuser}",response.Subject);
        Assert.Equal($"Name {uuser}, email {uuser}@null.net",response.Body);
        xConsole.WriteLine("Communication_valid_inputs: PASS");
    }
    [Fact]
    public async Task Communication_invalid_customer() {
        var db = Utils.GetMockDB();
        var customer = new CustomerService(db,logCustomer);
        var template = new TemplateService(db,logTemplate);
        var communication = new CommunicationService(customer,template,logCommunication);

        Guid customerId = Guid.NewGuid();
        Guid templateId = Guid.NewGuid();

        db.Templates.Add(new Template { Id = templateId,Name = "Template test",Subject = "String",Body = "String" });
        await db.SaveChangesAsync();
        var response = await communication.SendCommunication(customerId,templateId);
        Assert.False(response.Success);
        Assert.Equal($"Customer with ID '{customerId}' not found",response.Message);
        xConsole.WriteLine("Communication_invalid_customer: PASS");
    }
    [Fact]
    public async Task Communication_invalid_template() {
        var db = Utils.GetMockDB();
        var customer = new CustomerService(db,logCustomer);
        var template = new TemplateService(db,logTemplate);
        var communication = new CommunicationService(customer,template,logCommunication);

        Guid customerId = Guid.NewGuid();
        Guid templateId = Guid.NewGuid();

        db.Customers.Add(new Customer { Id = customerId,Name = "Tester",Email = "test@null.net" });
        await db.SaveChangesAsync();
        var response = await communication.SendCommunication(customerId,templateId);
        Assert.False(response.Success);
        Assert.Equal($"Template with ID '{templateId}' not found",response.Message);
        xConsole.WriteLine("Communication_invalid_template: PASS");
    }
}
