namespace PortSafe.DTOs
{
    public class CadastroMoradorRequestDTO
    {
        public string Nome { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string CPF { get; set; } = string.Empty;

        public string? Photo { get; set; }

        public int CondominioId { get; set; }  // ID do condomínio escolhido


        // Dados da unidade - apenas um deve ser preenchido

        public UnidadeCasaDTO? DadosCasa { get; set; }


        public UnidadeApartamentoDTO? DadosApartamento { get; set; }
    }
}