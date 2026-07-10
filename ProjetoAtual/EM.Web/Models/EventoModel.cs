namespace EM.Web.Models;

public class EventoModel
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public DateTime Data { get; set; }
    public string? Observacao { get; set; }
    public int TotalParticipantes { get; set; }
    public List<AlunoModel> Participantes { get; set; } = new();
    public List<AlunoModel> Disponiveis { get; set; } = new();
}
