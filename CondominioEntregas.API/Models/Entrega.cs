using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PortSafe.Models
{
    public class Entrega
    {
        public int Id { get; set; }

        [Required]
        public string? NomeDestinatario { get; set; }

        public string? NumeroCasa { get; set; }

        public string? EnderecoGerado { get; set; }

        public int? ArmariumId { get; set; }
        
        public virtual Armario? Armario { get; set; }

        public string? CodigoEntrega { get; set; }

        public string? SenhaAcesso { get; set; }

        public DateTime DataHoraRegistro { get; set; }

        public DateTime? DataHoraRetirada { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusEntrega Status { get; set; }

        public string? TelefoneWhatsApp { get; set; }

        public bool MensagemEnviada { get; set; }
    }

    public enum StatusEntrega
    {
        AguardandoValidacao,
        AguardandoArmario,
        Armazenada,
        Retirada,
        ErroValidacao,
        RedirecionadoPortaria
    }
}