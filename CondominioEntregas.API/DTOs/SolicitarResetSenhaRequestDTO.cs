using System.ComponentModel.DataAnnotations;

namespace PortSafe.DTOs
{
    public class SolicitarResetSenhaRequestDTO
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
    }
}
