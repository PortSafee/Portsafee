namespace PortSafe.DTOs;

public class LoginDTO
{
    public string? Username { get; set; } = string.Empty; // Pode ser email ou nome de usuário

    public string? Password { get; set; } = string.Empty; // Senha do usuário
}
