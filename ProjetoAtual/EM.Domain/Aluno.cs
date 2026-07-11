using EM.Domain.Interface;
using EM.Domain.Utilitarios;

namespace EM.Domain;

public class Aluno : IEntidade
{
    public int Matricula { get; set; }

    public string Nome { get; set; }

    public DateTime Nascimento { get; set; }

    public EnumeradorSexo Sexo { get; set; }

    /// <summary>Cidade em texto livre (ex: "São Paulo - SP").</summary>
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
            var idade = hoje.Year - Nascimento.Year;
            if (Nascimento.Date > hoje.AddYears(-idade)) idade--;
            return idade < 0 ? 0 : idade;
        }
    }
}
