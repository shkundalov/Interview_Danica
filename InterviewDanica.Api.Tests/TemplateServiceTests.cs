using InterviewDanica.Api.Data;
using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Models;
using InterviewDanica.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace InterviewDanica.Api.Tests;

public class TemplateServiceTests {
    private readonly ITestOutputHelper xConsole;
    private readonly ILogger<TemplateService> log;
    private readonly DBContext db;
    public TemplateServiceTests(ITestOutputHelper xConsole) {
        this.xConsole = xConsole;
        this.log = Utils.GetMockLog<TemplateService>();
        this.db = Utils.GetMockDB();
    }
    [Fact]
    public async Task Template_Create() {
        var template = new TemplateService(db,log);
        var request = new CreateTemplateRequestDTO {
            Name = "String",
            Subject = "String {{Name}} string",
            Body = "String {{Name}} string {{Email}} string"
        };
        var response = await template.CreateOrGetTemplate(request);
        Assert.NotNull(response);
        Assert.Equal("String",response.Name);
        Assert.Contains("{{Name}}",response.Body);
        xConsole.WriteLine("Template_Create: PASS");
    }
    [Fact]
    public async Task Template_Create_Duplicate() {
        var template = new TemplateService(db,log);
        var request = new CreateTemplateRequestDTO {
            Name = "String",
            Subject = "String {{Name}} string",
            Body = "String {{Name}} string {{Email}} string"
        };
        await template.CreateOrGetTemplate(request);
        request = new CreateTemplateRequestDTO {
            Name = "String",
            Subject = "String2 {{Name}} string2",
            Body = "String {{Name}} string {{Email}} string"
        };
        var response = await template.CreateOrGetTemplate(request);
        Assert.NotNull(response);
        Assert.Equal("String",response.Name);
        Assert.Equal("String {{Name}} string",response.Subject);
        xConsole.WriteLine("Template_Create_Duplicate: PASS");
    }
    [Fact]
    public async Task Template_GetAll() {
        var template = new TemplateService(db,log);
        var ustring = Utils.UString();
        db.Templates.Add(new Template { Id = Guid.NewGuid(),Name = ustring,Subject = "String",Body = "String" });
        db.Templates.Add(new Template { Id = Guid.NewGuid(),Name = $"{ustring}2",Subject = "String",Body = "String" });
        await db.SaveChangesAsync();
        var response = await template.GetAllTemplates();
        Assert.Equal(2,response.Count());
        xConsole.WriteLine("Template_GetAll: PASS");
    }
    [Fact]
    public async Task Template_GetById() {
        var template = new TemplateService(db,log);
        var templateId = Guid.NewGuid();
        var ustring = Utils.UString();
        db.Templates.Add(new Template { Id = templateId,Name = ustring,Subject = "String",Body = "String" });
        await db.SaveChangesAsync();
        var response = await template.GetTemplateById(templateId);
        Assert.NotNull(response);
        Assert.Equal(ustring,response.Name);
        xConsole.WriteLine("Template_GetById: PASS");
    }
    [Fact]
    public async Task Template_Update() {
        var template = new TemplateService(db,log);
        var templateId = Guid.NewGuid();
        var ustring = Utils.UString();
        db.Templates.Add(new Template { Id = templateId,Name = ustring,Subject = "String",Body = "String" });
        await db.SaveChangesAsync();
        var request = new UpdateTemplateRequestDTO { Name = $"{ustring}2",Subject = "String2",Body = "String2" };
        var response = await template.UpdateTemplate(templateId,request);
        Assert.True(response);
        var updated = await db.Templates.FindAsync(templateId);
        Assert.Equal($"{ustring}2",updated!.Name);
        xConsole.WriteLine("Template_Update: PASS");
    }
    [Fact]
    public async Task Template_Update_Duplicate() {
        var template = new TemplateService(db,log);
        var ustring = Utils.UString();
        db.Templates.Add(new Template { Id = Guid.NewGuid(),Name = ustring,Subject = "String",Body = "String" });
        await db.SaveChangesAsync();
        var request = new UpdateTemplateRequestDTO { Name = ustring,Subject = "String2",Body = "String2" };
        var response = await template.UpdateTemplate(Guid.NewGuid(),request);
        Assert.False(response);
        xConsole.WriteLine("Template_Update_Duplicate: PASS");
    }
    [Fact]
    public async Task Template_Delete() {
        var template = new TemplateService(db,log);
        var templateId = Guid.NewGuid();
        var ustring = Utils.UString();
        db.Templates.Add(new Template { Id = templateId,Name = ustring,Subject = "String",Body = "String" });
        await db.SaveChangesAsync();
        var response = await template.DeleteTemplate(templateId);
        Assert.True(response);
        var deleted = await db.Templates.FindAsync(templateId);
        Assert.Null(deleted);
        xConsole.WriteLine("Template_Delete: PASS");
    }
    [Fact]
    public async Task Template_Build() {
        var template = new TemplateService(db,log);
        var templateId = Guid.NewGuid();
        var ustring = Utils.UString();
        var uuser = Utils.UString();
        db.Templates.Add(new Template {
            Id = templateId,
            Name = ustring,
            Subject = "Subject {{Name}}",
            Body = "String {{Name}} string {{Email}} string"
        });
        await db.SaveChangesAsync();
        var customer = new CustomerResponseDTO { Id = Guid.NewGuid(),Name = uuser,Email = $"{uuser}@null.net" };
        var templateRecord = await template.GetTemplateById(templateId);
        var (subject,body) = await template.BuildTemplate(templateRecord!,customer);
        Assert.Equal($"Subject {uuser}",subject);
        Assert.Equal($"String {uuser} string {uuser}@null.net string",body);
        xConsole.WriteLine("Template_Build: PASS");
    }
}
