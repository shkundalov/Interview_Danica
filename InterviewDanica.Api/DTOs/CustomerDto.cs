namespace InterviewDanica.Api.DTOs;
public class CreateCustomerRequestDTO {
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}
public class UpdateCustomerRequestDTO {
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}
public class CustomerResponseDTO {
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}
