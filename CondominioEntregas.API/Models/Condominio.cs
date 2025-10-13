namespace Endereco.Models;

public abstract class Condominio
{
    public string? Nome { get; set; }

    public string? Endereco { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    public string? CEP { get; set; }
}