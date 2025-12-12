using PortSafe.Models;

namespace PortSafe.DTOs
{
    public class EntregaResponseDTO
    {
        public int Id { get; set; }
        public string? NomeDestinatario { get; set; }
        public string? NumeroCasa { get; set; }
        public string? EnderecoGerado { get; set; }
        public string? CodigoEntrega { get; set; }
        public string? SenhaAcesso { get; set; }
        public DateTime DataHoraRegistro { get; set; }
        public DateTime? DataHoraRetirada { get; set; }
        public StatusEntrega Status { get; set; }
        public string? TelefoneWhatsApp { get; set; }
        public bool MensagemEnviada { get; set; }
        
        // Dados do armário (sem referência circular)
        public int? ArmarioId { get; set; }
        public string? ArmarioNumero { get; set; }
        public StatusArmario? ArmarioStatus { get; set; }
    }
}
