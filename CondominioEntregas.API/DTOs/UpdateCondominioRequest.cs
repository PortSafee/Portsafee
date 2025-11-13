namespace PortSafe.DTOs
{
    public class UpdateCondominioRequest
    {
        public string NomeDoCondominio { get; set; } = string.Empty;
        public string? Tipo { get; set; } // "Casa" ou "Apartamento"
    }
}
