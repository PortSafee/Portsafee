public enum StatusEntrega
{
  Pendente,
  Entregue,
  Cancelada
}

public class EntregaResponse
{
  public int Id { get; set; }
  public string? NomeDestinatario { get; set; }
  public string? NumeroCasa { get; set; }
  public string? EnderecoGerado { get; set; }
  public StatusEntrega Status { get; set; }
}