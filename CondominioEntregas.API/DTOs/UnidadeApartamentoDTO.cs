namespace PortSafe.DTOs
{
    public class UnidadeApartamentoDTO : UnidadeDTO
    {
        public string Bloco { get; set; } = string.Empty;
        public string NumeroApartamento { get; set; } = string.Empty;

        public UnidadeApartamentoDTO()
        {
            TipoUnidade = "Apartamento";
        }
    }
}