using EM.Domain;

namespace EM.Web.Models;

public class RelatorioModel
{
    public List<RelatorioTurmaModel> Turmas { get; set; } = new();
    public List<ResumoPresencaAluno> Alunos { get; set; } = new();
}

public class RelatorioTurmaModel
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public int TotalAlunos { get; set; }
    public int Encontros { get; set; }
    public DateTime? UltimoEncontro { get; set; }

    /// <summary>Presença média considerando todos os encontros da turma.</summary>
    public int PercentualMedio { get; set; }
}
