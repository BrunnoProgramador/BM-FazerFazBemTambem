using EM.Domain.Interface;

namespace EM.Domain;

public class Turma : IEntidade
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public int Ano { get; set; }

    /// <summary>Nome do professor responsável (opcional).</summary>
    public string? Professor { get; set; }

    /// <summary>Dias da semana em que a turma ocorre, ex: "Segunda, Quarta" (opcional).</summary>
    public string? DiasSemana { get; set; }

    /// <summary>Horário da aula, ex: "14:00" (opcional).</summary>
    public string? Horario { get; set; }

    public int TotalAlunos { get; set; }
    public List<Aluno> Alunos { get; set; } = new();
}
