namespace PortSafe.Models;

public class Condominio
{

    public int Id { get; set; }

    public string? NomeDoCondominio { get; set; }

    public string? Tipo { get; set; } // "Casa" ou "Apartamento"

    public virtual ICollection<Morador> Moradores { get; set; } = new List<Morador>(); // Relação um-para-muitos com Morador
    
    public virtual ICollection<Porteiro> Porteiros { get; set; } = new List<Porteiro>(); // Relação um-para-muitos com Porteiro
}