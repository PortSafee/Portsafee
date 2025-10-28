namespace PortSafe.DTOs
{
    public class AcionarPortariaResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public int? ProtocoloAtendimento { get; set; }
        public DateTime? DataHoraAcionamento { get; set; }
    }
}
