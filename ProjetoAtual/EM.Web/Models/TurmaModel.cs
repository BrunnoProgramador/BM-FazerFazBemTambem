namespace EM.Web.Models;

public class TurmaModel
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public int Ano { get; set; }
    public string? Professor { get; set; }
    public string? DiasSemana { get; set; }
    public string? Horario { get; set; }
    public int TotalAlunos { get; set; }
    public List<AlunoModel> Alunos { get; set; } = new();
    public List<AlunoModel> AlunosForaDaTurma { get; set; } = new();

    /// <summary>Dias selecionados no formulário (checkboxes).</summary>
    public List<string> DiasSelecionados =>
        string.IsNullOrWhiteSpace(DiasSemana)
            ? new List<string>()
            : DiasSemana.Split(',').Select(d => d.Trim()).Where(d => d.Length > 0).ToList();
}
