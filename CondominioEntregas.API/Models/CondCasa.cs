namespace PortSafe.Models;

// Classe que herda de Condominio

public class CondCasa : Condominio 
{
    public string? Rua { get; set; }

    public int NumeroCasa { get; set; }
    
}