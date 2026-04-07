using InterviewDanica.Api.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace InterviewDanica.Api.Tests;

public class CustomersIntegrationTests {
    private readonly ITestOutputHelper xConsole;
    private readonly ApiClient<CustomerResponseDTO> api;

    public CustomersIntegrationTests(ITestOutputHelper xConsole) {
        this.xConsole = xConsole;
        this.api = new ApiClient<CustomerResponseDTO>("customers");
    }

    [Fact]
    public async Task IntegrationFlowTest_Customer() {
        xConsole.WriteLine("INIT: Receive credentials");
        await this.api.authorize();

        xConsole.WriteLine("STEP 1: GET all customers - expect empty");
        var customers = await api.GetAll();
        Assert.Empty(customers);

        xConsole.WriteLine("STEP 2: POST invalid email - expect 400");
        var invalid = new CreateCustomerRequestDTO { Name = "Tester",Email = "test_null.net" };
        var invalidResponse = await api.Post(invalid);
        Assert.Equal(HttpStatusCode.BadRequest,invalidResponse.StatusCode);

        xConsole.WriteLine("STEP 3: POST valid customer - expect 200");
        var valid = new CreateCustomerRequestDTO { Name = "Tester",Email = "test@null.net" };
        var validResponse = await api.PostAndRead(valid);
        Assert.Equal("Tester",validResponse.Name);
        Assert.Equal("test@null.net",validResponse.Email);
        var id = validResponse.Id;

        xConsole.WriteLine("STEP 4: POST same email - expect existing customer");
        var idempotent = new CreateCustomerRequestDTO { Name = "Other",Email = "test@null.net" };
        var existing = await api.PostAndRead(idempotent);
        Assert.Equal(id,existing.Id);
        Assert.Equal("Tester",existing.Name);

        xConsole.WriteLine("STEP 5: GET all - expect 1 record");
        var list = await api.GetAll();
        Assert.Single(list);
        Assert.Equal(id,list[0].Id);

        xConsole.WriteLine("STEP 6: GET by ID - expect record exists");
        var byId = await api.GetById(id);
        Assert.Equal(id,byId.Id);

        xConsole.WriteLine("STEP 7: PUT invalid email - expect 400");
        var updateInvalid = new UpdateCustomerRequestDTO { Name = "Tester",Email = "tester_null.net" };
        var updateInvalidResponse = await api.Put(id,updateInvalid);
        Assert.Equal(HttpStatusCode.BadRequest,updateInvalidResponse.StatusCode);

        xConsole.WriteLine("STEP 8: PUT valid update - expect 204");
        var updateValid = new UpdateCustomerRequestDTO { Name = "Tester",Email = "tester@null.net" };
        var updateValidResponse = await api.Put(id,updateValid);
        Assert.Equal(HttpStatusCode.NoContent,updateValidResponse.StatusCode);

        xConsole.WriteLine("STEP 9: PUT same data again - expect 204");
        var updateIdempotentResponse = await api.Put(id,updateValid);
        Assert.Equal(HttpStatusCode.NoContent,updateIdempotentResponse.StatusCode);

        xConsole.WriteLine("STEP 10: GET all and validate updated data");
        var updatedList = await api.GetAll();
        Assert.Single(updatedList);
        Assert.Equal("Tester",updatedList[0].Name);
        Assert.Equal("tester@null.net",updatedList[0].Email);

        xConsole.WriteLine("STEP 11: DELETE customer - expect 200");
        var delete1 = await api.Delete(id);
        Assert.Equal(HttpStatusCode.OK,delete1.StatusCode);

        xConsole.WriteLine("STEP 12: DELETE again - expect 200");
        var delete2 = await api.Delete(id);
        Assert.Equal(HttpStatusCode.OK,delete2.StatusCode);

        xConsole.WriteLine("FINAL: GET all - expect empty");
        var finalList = await api.GetAll();
        Assert.Empty(finalList);
    }
}

public class TemplatesIntegrationTests {
    private readonly ApiClient<TemplateResponseDTO> api = new ApiClient<TemplateResponseDTO>("templates");
    private readonly ITestOutputHelper xConsole;

    public TemplatesIntegrationTests(ITestOutputHelper xConsole) {
        this.xConsole = xConsole;
    }

    [Fact]
    public async Task IntegrationFlowTest_Template() {
        xConsole.WriteLine("INIT: Receive credentials");
        await this.api.authorize();

        xConsole.WriteLine("STEP 1: GET all - expect empty");
        var templates = await api.GetAll();
        Assert.Empty(templates);

        xConsole.WriteLine("STEP 2: POST template - expect 200");
        var createReq = new {
            name = "String",
            subject = "String",
            body = "String {{Name}} string {{Email}} string"
        };
        var created = await api.PostAndRead(createReq);
        var id = created.Id;
        Assert.Equal("String",created.Name);
        Assert.Equal("String",created.Subject);

        xConsole.WriteLine("STEP 3: POST same name (idempotent) - expect 200");
        var idempotentReq = new {
            name = "String",
            subject = "String",
            body = "String {{Name}} string {{Email}} string"
        };
        var existing = await api.PostAndRead(idempotentReq);
        Assert.Equal(id,existing.Id);
        Assert.Equal("String",existing.Subject);

        xConsole.WriteLine("STEP 4: GET all - expect 1");
        var list = await api.GetAll();
        Assert.Single(list);
        Assert.Equal(id,list[0].Id);

        xConsole.WriteLine("STEP 5: GET by ID - expect 200");
        var byId = await api.GetById(id);
        Assert.Equal(id,byId.Id);

        xConsole.WriteLine("STEP 6: PUT update - expect 204");
        var updateReq = new {
            name = "String",
            subject = "String2",
            body = "String2 {{Name}} string2 {{Email}} string2"
        };
        var updateResponse = await api.Put(id,updateReq);
        Assert.Equal(HttpStatusCode.NoContent,updateResponse.StatusCode);

        xConsole.WriteLine("STEP 7: PUT data again (idempotent) - expected 204");
        var updateResponse2 = await api.Put(id,updateReq);
        Assert.Equal(HttpStatusCode.NoContent,updateResponse2.StatusCode);

        xConsole.WriteLine("STEP 8: GET all and validate update - expected 200");
        var updatedList = await api.GetAll();
        Assert.Single(updatedList);
        Assert.Equal("String",updatedList[0].Name);
        Assert.Equal("String2",updatedList[0].Subject);

        xConsole.WriteLine("STEP 9: DELETE - expect 200");
        var del1 = await api.Delete(id);
        Assert.Equal(HttpStatusCode.OK,del1.StatusCode);

        xConsole.WriteLine("STEP 10: DELETE again (idempotent) - expect 200");
        var del2 = await api.Delete(id);
        Assert.Equal(HttpStatusCode.OK,del2.StatusCode);

        xConsole.WriteLine("FINAL: GET all - expect empty");
        var final = await api.GetAll();
        Assert.Empty(final);
    }
}
