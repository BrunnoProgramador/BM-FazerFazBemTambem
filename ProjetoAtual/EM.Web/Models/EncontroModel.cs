namespace EM.Web.Models;

public class EncontroModel
{
    public int Codigo { get; set; }
    public int TurmaCodigo { get; set; }
    public string? TurmaNome { get; set; }
    public DateTime Data { get; set; }
    public string? Conteudo { get; set; }
    public int TotalAlunos { get; set; }
    public int TotalPresentes { get; set; }
    public List<PresencaModel> Presencas { get; set; } = new();

    public int Percentual =>
        TotalAlunos == 0 ? 0 : (int)Math.Round(100.0 * TotalPresentes / TotalAlunos);
}

public class PresencaModel
{
    public int Matricula { get; set; }
    public string Nome { get; set; }
    public bool Presente { get; set; }
}
