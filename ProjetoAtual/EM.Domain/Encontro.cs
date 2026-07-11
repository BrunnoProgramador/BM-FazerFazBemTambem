using EM.Domain.Interface;

namespace EM.Domain;

/// <summary>
/// Um encontro da turma em uma data, com a lista de presença.
/// É a unidade básica de frequência do projeto.
/// </summary>
public class Encontro : IEntidade
{
    public int Codigo { get; set; }
    public Turma Turma { get; set; }
    public DateTime Data { get; set; }

    /// <summary>Conteúdo trabalhado no encontro (lançamento de conteúdo).</summary>
    public string? Conteudo { get; set; }

    public List<PresencaAluno> Presencas { get; set; } = new();

    /// <summary>Total de alunos na lista (preenchido nas listagens).</summary>
    public int TotalAlunos { get; set; }

    /// <summary>Total de presentes (preenchido nas listagens).</summary>
    public int TotalPresentes { get; set; }

    public int PercentualPresenca =>
        TotalAlunos == 0 ? 0 : (int)Math.Round(100.0 * TotalPresentes / TotalAlunos);
}

/// <summary>Presença de um aluno em um encontro.</summary>
public class PresencaAluno
{
    public Aluno Aluno { get; set; }
    public bool Presente { get; set; }
}

/// <summary>Registro de presença do ponto de vista do aluno (perfil).</summary>
public class RegistroPresencaAluno
{
    public DateTime Data { get; set; }
    public bool Presente { get; set; }
    public string TurmaNome { get; set; }
}

/// <summary>Resumo de presença por aluno (relatórios).</summary>
public class ResumoPresencaAluno
{
    public int Matricula { get; set; }
    public string Nome { get; set; }
    public int StatusCodigo { get; set; }
    public int TotalEncontros { get; set; }
    public int TotalPresencas { get; set; }
    public DateTime? UltimaPresenca { get; set; }

    public int Percentual =>
        TotalEncontros == 0 ? 0 : (int)Math.Round(100.0 * TotalPresencas / TotalEncontros);
}
