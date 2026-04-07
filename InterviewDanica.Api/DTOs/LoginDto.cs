namespace InterviewDanica.Api.DTOs;
public class LoginRequestDTO {
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
public class LoginResponseDTO {
    public string Token { get; set; } = null!;
}