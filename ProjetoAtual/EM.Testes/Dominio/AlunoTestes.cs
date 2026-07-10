using EM.Domain;
using Xunit;

namespace EM.Testes.Dominio;

public class AlunoTestes
{
    [Fact]
    public void Idade_de_quem_faz_10_anos_hoje_eh_10()
    {
        var aluno = new Aluno { Nascimento = DateTime.Today.AddYears(-10) };
        Assert.Equal(10, aluno.Idade);
    }

    [Fact]
    public void Idade_de_quem_faz_10_anos_amanha_ainda_eh_9()
    {
        var aluno = new Aluno { Nascimento = DateTime.Today.AddYears(-10).AddDays(1) };
        Assert.Equal(9, aluno.Idade);
    }

    [Fact]
    public void Idade_nunca_eh_negativa()
    {
        var aluno = new Aluno { Nascimento = DateTime.Today.AddYears(1) };
        Assert.Equal(0, aluno.Idade);
    }
}
