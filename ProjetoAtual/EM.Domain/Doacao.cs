using EM.Domain.Interface;

namespace EM.Domain;

/// <summary>Doação recebida pelo projeto (ex: "50 cadernos").</summary>
public class Doacao : IEntidade
{
    public int Codigo { get; set; }
    public string Item { get; set; }
    public int? Quantidade { get; set; }
    public DateTime Data { get; set; }
    public string? Doador { get; set; }
}
