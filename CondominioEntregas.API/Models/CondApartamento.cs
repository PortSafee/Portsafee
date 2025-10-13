namespace Endereco.Models;

public class CondApartamento : Condominio // Classe que herda de Condominio
{
    public string? Bloco { get; set; }

    public string? NumeroApartamento { get; set; }
}