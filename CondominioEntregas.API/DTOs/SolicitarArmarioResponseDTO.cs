namespace PortSafe.DTOs
{
    public class SolicitarArmarioResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public int? NumeroArmario { get; set; }
        public string? CodigoEntrega { get; set; }
        public int? EntregaId { get; set; }
        public DateTime? LimiteDeposito { get; set; }
    }
}
