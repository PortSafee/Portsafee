namespace PortSafe.DTOs
{
    public class UnidadeCasaDTO : UnidadeDTO
    {
        public string Rua { get; set; } = string.Empty;
        public int NumeroCasa { get; set; }
        public string CEP { get; set; } = string.Empty;

    }
}
        
