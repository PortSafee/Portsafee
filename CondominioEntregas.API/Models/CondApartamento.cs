namespace PortSafe.Models;

// Classe que herda de Condominio

public class CondApartamento : Condominio 
{
    public string? Bloco { get; set; }

    public string? NumeroApartamento { get; set; }
    
}