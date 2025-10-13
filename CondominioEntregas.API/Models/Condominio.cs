namespace PortSafe.Models;

public abstract class Condominio
{

    public int Id { get; set; }

    public string? Nome { get; set; }

    public string? Endereco { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    public string? CEP { get; set; }

    public virtual ICollection<Morador> Moradores { get; set; } = new List<Morador>(); // Relação um-para-muitos com Morador
}