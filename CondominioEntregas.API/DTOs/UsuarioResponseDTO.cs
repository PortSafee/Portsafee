namespace PortSafe.DTOs
{
    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Morador" ou "Porteiro"
        public DateTime DataCriacao { get; set; }
    }
}