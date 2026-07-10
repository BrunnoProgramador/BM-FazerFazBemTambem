using EM.Domain.Interface;

namespace EM.Domain;

/// <summary>Anotação livre sobre o aluno (ex: "muito tímido", "possui alergia").</summary>
public class ObservacaoAluno : IEntidade
{
    public int Codigo { get; set; }
    public int AlunoMatricula { get; set; }
    public DateTime Data { get; set; }
    public string Texto { get; set; }
}
