namespace PortSafe.DTOs;

public class CondominioResponse
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public string? Endereco { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    public string? CEP { get; set; }

    public string? Discriminator { get; set; } // "CondApartamento" ou "CondCasa"

    public string? Bloco { get; set; }

    public string? NumeroApartamento { get; set; }

    public int? NumeroCasa { get; set; }

    public string? Rua { get; set; }
}
