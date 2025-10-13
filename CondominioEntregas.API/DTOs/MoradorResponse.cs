using PortSafe.DTOs;

namespace PortSafe.DTOs;

public class MoradorResponse
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public CondominioResponse Condominio { get; set; } = new CondominioResponse(); // Propriedade para o condomínio

    public string? Bloco { get; set; } // Apenas para apartamentos

    public string? NumeroApartamento { get; set; } // Apenas para apartamentos

    public int? NumeroCasa { get; set; } // Apenas para casas

}
