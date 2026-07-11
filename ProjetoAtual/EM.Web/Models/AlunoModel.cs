using EM.Domain;
using EM.Domain.Utilitarios;

namespace EM.Web.Models;

public class AlunoModel
{
    public int Matricula { get; set; }
    public string Nome { get; set; }
    public EnumeradorSexo Sexo { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? Cidade { get; set; }
    public string? Bairro { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? ResponsavelTelefone { get; set; }
    public string? ResponsavelParentesco { get; set; }
    public EnumeradorStatusAluno Status { get; set; } = EnumeradorStatusAluno.Ativo;

    public int Idade
    {
        get
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - DataNascimento.Year;
            if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
            return idade < 0 ? 0 : idade;
        }
    }
}

/// <summary>Tudo que um voluntário precisa saber sobre a criança em uma tela.</summary>
public class PerfilAlunoModel
{
    public AlunoModel Aluno { get; set; }
    public List<TurmaModel> Turmas { get; set; } = new();
    public List<RegistroPresencaAluno> Presencas { get; set; } = new();
    public List<ObservacaoAluno> Observacoes { get; set; } = new();
    public List<HistoricoAluno> Historico { get; set; } = new();

    public int TotalPresencas => Presencas.Count(p => p.Presente);

    public int PercentualPresenca =>
        Presencas.Count == 0 ? 0 : (int)Math.Round(100.0 * TotalPresencas / Presencas.Count);

    public DateTime? UltimaPresenca =>
        Presencas.Where(p => p.Presente).Select(p => (DateTime?)p.Data).FirstOrDefault();
}
