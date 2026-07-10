using EM.Domain.Interface;

namespace EM.Domain;

/// <summary>Evento do projeto (passeio, festa junina, natal...).</summary>
public class Evento : IEntidade
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public DateTime Data { get; set; }
    public string? Observacao { get; set; }
    public int TotalParticipantes { get; set; }
    public List<Aluno> Participantes { get; set; } = new();
}
