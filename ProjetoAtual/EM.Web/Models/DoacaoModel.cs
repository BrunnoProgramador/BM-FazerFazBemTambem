namespace EM.Web.Models;

public class DoacaoModel
{
    public int Codigo { get; set; }
    public string Item { get; set; }
    public int? Quantidade { get; set; }
    public DateTime Data { get; set; }
    public string? Doador { get; set; }
}
