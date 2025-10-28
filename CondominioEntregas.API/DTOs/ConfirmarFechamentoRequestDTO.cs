using System.ComponentModel.DataAnnotations;

namespace PortSafe.DTOs
{
    public class ConfirmarFechamentoRequestDTO
    {
        [Required(ErrorMessage = "ID da entrega é obrigatório")]
        public int EntregaId { get; set; }

        // Opcional: URL da foto do armário fechado
        public string? UrlFoto { get; set; }
    }
}
