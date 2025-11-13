namespace PortSafe.DTOs
{
    public class ValidacaoDestinatarioResponseDTO
    {
        public bool Validado { get; set; }
        
        public string Mensagem { get; set; } = string.Empty;
        
        public TipoResultadoValidacao TipoResultado { get; set; }
        
        // Dados encontrados (para comparação visual)
        public DadosDestinatarioDTO? DadosEncontrados { get; set; }
        
        // Opções disponíveis para o motorista
        public bool PodeRetentar { get; set; }
        
        public bool PodeAcionarPortaria { get; set; }
        
        // Token para próxima etapa (solicitar armário)
        public string? TokenValidacao { get; set; }
        
        // ID da validação para rastreamento
        public int? ValidacaoId { get; set; }
    }

    public class DadosDestinatarioDTO
    {
        public string? NomeMorador { get; set; }
        public string? TelefoneWhatsApp { get; set; }
        public string? TipoUnidade { get; set; } // "Casa" ou "Apartamento"
        public string? Endereco { get; set; } // "Rua X, Casa 10" ou "Torre A, Apto 301"
        public string? CEP { get; set; }
        public int UnidadeId { get; set; }
        public int MoradorId { get; set; }
    }

    public enum TipoResultadoValidacao
    {
        Sucesso,              // Nome e CEP conferem perfeitamente
        DivergenciaNome,      // CEP correto, mas nome diferente
        DivergenciaCEP,       // Nome similar, mas CEP diferente
        NaoEncontrado,        // Nenhum registro encontrado
        MultiplasCombinacoes  // Vários moradores com nomes similares
    }
}
