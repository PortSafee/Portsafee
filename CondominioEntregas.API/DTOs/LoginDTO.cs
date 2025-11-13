namespace PortSafe.DTOs;

public class LoginDTO
{
    public string? UsernameOrEmail { get; set; } = string.Empty; // Nome de usuário ou email

    public string? Password { get; set; } = string.Empty; // Senha do usuário
}
