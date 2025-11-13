namespace PortSafe.DTOs
{
    public class ConfirmarFechamentoResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? CodigoEntrega { get; set; }
        public string? SenhaAcesso { get; set; }
        public DateTime? DataHoraEntrega { get; set; }
        public bool NotificacaoEnviada { get; set; }
    }
}
