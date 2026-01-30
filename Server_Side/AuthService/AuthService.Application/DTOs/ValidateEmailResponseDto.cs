namespace AuthService.Application.DTOs
{
    public class ValidateEmailResponseDto
    {
       public string Email { get; set; } = string.Empty;   
       public bool IsValid { get; set; }
    }
}
