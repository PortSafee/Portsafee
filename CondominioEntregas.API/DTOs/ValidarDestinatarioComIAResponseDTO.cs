using PortSafe.Services.AI.Models;

namespace PortSafe.DTOs
{
    public class ValidarDestinatarioComIAResponseDTO
    {
        public bool Validado { get; set; }
        public double ConfiancaIA { get; set; }
        public string? Mensagem { get; set; }
        public TipoResultadoValidacao TipoResultado  { get; set; }
        public DadosDestinatarioDTO? DadosEncontrados { get; set; }
        public List<SuggestedMatch>? Sugestoes { get; set; }
        public bool PodeRetentar { get; set; }
        public bool PodeAcionarPortaria { get; set; }
        public string? TokenValidacao { get; set; }
        public int? ValidacaoId { get; set; }
    }

    public class TipoResultadoValidacaoo
    {
        public const string Sucesso = "Sucesso";
        public const string FalhaValidacao = "FalhaValidacao";
        public const string ErroSistema = "ErroSistema";
        public const string NaoEncontrado = "NaoEncontrado";
        public const string PossivelMatch = "PossivelMatch";
    }
}
