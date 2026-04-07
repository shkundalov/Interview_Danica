using InterviewDanica.Api.Data;
using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InterviewDanica.Api.Services;
/*
 * In log an accurate states are recorded for reusability purpose in possible other services, yet Controller sets idempotant states
 */
public interface ITemplateService {
    Task<IEnumerable<TemplateResponseDTO>> GetAllTemplates();
    Task<TemplateResponseDTO?> GetTemplateById(Guid id);
    Task<TemplateResponseDTO?> _GetTemplateByName(string name); //Internal, used for duplicates, can be reused in future
    Task<TemplateResponseDTO> CreateOrGetTemplate(CreateTemplateRequestDTO request);
    Task<TemplateResponseDTO> _CreateTemplate(CreateTemplateRequestDTO request);
    Task<bool> UpdateTemplate(Guid id,UpdateTemplateRequestDTO request);
    Task<bool> DeleteTemplate(Guid id);
    Task<(string RenderedSubject,string RenderedBody)> BuildTemplate(TemplateResponseDTO templateId,CustomerResponseDTO customer);
}

public class TemplateService : ITemplateService {
    private readonly DBContext db;
    private readonly ILogger<TemplateService> log;
    private static TemplateResponseDTO BuildResponse(Template record) {
        return new TemplateResponseDTO {
            Id = record.Id,
            Name = record.Name,
            Subject = record.Subject,
            Body = record.Body
        };
    }
    public TemplateService(DBContext context,ILogger<TemplateService> logger) {
        db = context;
        log = logger;
    }
    public async Task<IEnumerable<TemplateResponseDTO>> GetAllTemplates() {
        log.LogInformation("REQ: GET api/Templates/");
        var records = await db.Templates.ToListAsync();
        var response = records.Select(BuildResponse);
        log.LogInformation("RES: GET api/Templates/ - 200, {Count}",records.Count);
        return response;
    }
    public async Task<TemplateResponseDTO?> GetTemplateById(Guid id) {
        log.LogInformation("REQ: GET api/Templates/{id}",id);
        var record = await db.Templates.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null) {
            log.LogWarning("RES: GET api/Templates/{id} - 404",id);
            return null;
        }
        log.LogInformation("RES: GET api/Templates/{id} - 200, {name}",id,record.Name);
        return BuildResponse(record);
    }
    public async Task<TemplateResponseDTO?> _GetTemplateByName(string name) {
        log.LogInformation("REQ: GET api/Templates/{name}",name);
        var record = await db.Templates.FirstOrDefaultAsync(r => r.Name == name);
        if (record == null) {
            log.LogWarning("RES: GET api/Templates/{name} - 404",name);
            return null;
        }
        log.LogInformation("RES: GET api/Templates/{name} - 200",record.Name);
        return BuildResponse(record);
    }
    public async Task<TemplateResponseDTO> CreateOrGetTemplate(CreateTemplateRequestDTO request) {
        var response = await this._GetTemplateByName(request.Name);
        if (response != null) {
            return response;
        }
        return await this._CreateTemplate(request);
    }
    public async Task<TemplateResponseDTO> _CreateTemplate(CreateTemplateRequestDTO request) {
        log.LogInformation("REQ: POST api/Templates {name}",request.Name);
        var record = new Template {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Subject = request.Subject,
            Body = request.Body
        };
        db.Templates.Add(record);
        await db.SaveChangesAsync();
        log.LogInformation("RES: POST​ api​/Templates - 200, {id}",record.Id);
        return BuildResponse(record);
    }
    public async Task<bool> UpdateTemplate(Guid id,UpdateTemplateRequestDTO request) {
        log.LogInformation("REQ: PUT api/Templates {id}",id);
        var record = await db.Templates.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null) {
            log.LogWarning("RES: PUT api​/Templates​ {id} - 404",id);
            return false;
        }
        var nameTaken = await db.Templates.AnyAsync(r => r.Name == request.Name && r.Id != id);
        if (nameTaken) {
            log.LogWarning("RES: PUT api​/Templates​ {id} - 409, {name}",id,request.Name);
            throw new InvalidOperationException($"Template '{request.Name}' already exists");
        }
        record.Name = request.Name;
        record.Subject = request.Subject;
        record.Body = request.Body;
        db.Templates.Update(record);
        await db.SaveChangesAsync();
        log.LogInformation("RES: PUT api​/Templates​ {id} - 200",id);
        return true;
    }
    public async Task<bool> DeleteTemplate(Guid id) {
        log.LogInformation("DELETE api​/Templates​ {id}",id);
        var record = await db.Templates.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null) {
            log.LogWarning("DELETE api​/Templates​ {id} - 404",id);
            return false;
        }
        db.Templates.Remove(record);
        await db.SaveChangesAsync();
        log.LogInformation("DELETE api​/Templates​ {id} - 200",id);
        return true;
    }
    public async Task<(string RenderedSubject,string RenderedBody)> BuildTemplate(TemplateResponseDTO template,CustomerResponseDTO customer) {
        var renderedSubject = ReplacePlaceholders(template.Subject,customer);
        var renderedBody = ReplacePlaceholders(template.Body,customer);
        return (renderedSubject,renderedBody);
    }
    private string ReplacePlaceholders(string text,CustomerResponseDTO customer) {
        text = Regex.Replace(text,@"\{\{Name\}\}",customer.Name,RegexOptions.IgnoreCase);
        text = Regex.Replace(text,@"\{\{Email\}\}",customer.Email,RegexOptions.IgnoreCase);
        return text;
    }
}
