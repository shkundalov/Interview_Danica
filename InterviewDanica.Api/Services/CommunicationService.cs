using InterviewDanica.Api.DTOs;

namespace InterviewDanica.Api.Services;

/*
 * Communication sending is build using defined DTOs and Services to make a single source of truth for all logic and have sufficnet logs for all operations
 * Emails sending is implemented as a mock, but the logic is ready to be replaced with a real service
 */
public interface ICommunicationService {
    Task<SendCommunicationResponseDTO> SendCommunication(Guid customerId,Guid templateId);
}

public class CommunicationService : ICommunicationService {
    private readonly ITemplateService templateService;
    private readonly ICustomerService customerService;
    private readonly ILogger<CommunicationService> log;

    public CommunicationService(ICustomerService customerService,ITemplateService templateService,ILogger<CommunicationService> logger) {
        this.templateService = templateService;
        this.customerService = customerService;
        log = logger;
    }
    public async Task<SendCommunicationResponseDTO> SendCommunication(Guid customerId,Guid templateId) {
        log.LogInformation("REQ: POST api​/Communications​/send {customerId}, {templateId}",customerId,templateId);
        var customer = await customerService.GetCustomerById(customerId);
        if (customer == null) {
            log.LogWarning("RES: POST api​/Communications​/send {customerId} - 404, no customer",customerId);
            return new SendCommunicationResponseDTO {
                Success = false,
                Message = $"Customer with ID '{customerId}' not found"
            };
        }
        var template = await templateService.GetTemplateById(templateId);
        if (template == null) {
            log.LogWarning("RES: POST api​/Communications​/send {templateId} - 404, no template",templateId);
            return new SendCommunicationResponseDTO {
                Success = false,
                Message = $"Template with ID '{templateId}' not found"
            };
        }
        var (Subject,Body) = await templateService.BuildTemplate(template,customer);
        log.LogInformation("RES: POST api​/Communications​/send {customerId}, {templateId} - 200",customerId,templateId);

        // Emulate a mock email
        Console.WriteLine("=== Email Sending Mock ===");
        Console.WriteLine($"To:      {customer.Name} email:{customer.Email}");
        Console.WriteLine($"Subject: {Subject}");
        Console.WriteLine($"Body:    {Body}");
        Console.WriteLine("=== Email Sending Mock ===");

        return new SendCommunicationResponseDTO {
            Success = true,
            Message = $"Email sent to {customer.Email}",
            Subject = Subject,
            Body = Body
        };
    }
}
