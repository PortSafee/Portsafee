using System.ComponentModel.DataAnnotations;

namespace CondominioArmarios.API.DTOs
{
    // Request DTOs

    public class RegistrarEntregaRequest
    {
        [Required(ErrorMessage = "Nome do destinatário é obrigatório")]
        public string? NomeDestinatario { get; set; }
        
        [Required(ErrorMessage = "Número da casa é obrigatório")]
        public string? NumeroCasa { get; set; }
    }

    public class ConfirmarEnderecoRequest
    {
        [Required]
        public int EntregaId { get; set; }
        
        [Required]
        public bool EnderecoCorreto { get; set; }
    }

    public class FechamentoArmarioRequest
    {
        [Required]
        public int ArmariumId { get; set; }
    }

    public class RetirarEntregaRequest
    {
        [Required(ErrorMessage = "Senha de acesso é obrigatória")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Senha deve ter 6 dígitos")]
        public string? SenhaAcesso { get; set; }
    }
}