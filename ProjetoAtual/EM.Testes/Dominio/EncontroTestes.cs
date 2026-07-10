using EM.Domain;
using Xunit;

namespace EM.Testes.Dominio;

public class EncontroTestes
{
    [Theory]
    [InlineData(22, 18, 82)] // 18/22 = 81,8% → arredonda para 82
    [InlineData(10, 10, 100)]
    [InlineData(10, 0, 0)]
    [InlineData(0, 0, 0)]   // sem alunos não divide por zero
    [InlineData(3, 2, 67)]  // 66,7% → 67
    public void Percentual_de_presenca_calcula_e_arredonda(int total, int presentes, int esperado)
    {
        var encontro = new Encontro { TotalAlunos = total, TotalPresentes = presentes };
        Assert.Equal(esperado, encontro.PercentualPresenca);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(38, 29, 76)]
    public void Resumo_por_aluno_calcula_percentual(int encontros, int presencas, int esperado)
    {
        var resumo = new ResumoPresencaAluno
        {
            TotalEncontros = encontros,
            TotalPresencas = presencas
        };
        Assert.Equal(esperado, resumo.Percentual);
    }
}
