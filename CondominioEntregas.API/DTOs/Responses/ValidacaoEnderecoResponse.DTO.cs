public class ValidacaoEnderecoResponse
    {
        public int EntregaId { get; set; }
        public string? EnderecoGerado { get; set; }
        public string? NomeDestinatario { get; set; }
        public string? NumeroCasa { get; set; }
        public bool PodeRedigitar { get; set; }
        public int TentativasRestantes { get; set; }
    }
