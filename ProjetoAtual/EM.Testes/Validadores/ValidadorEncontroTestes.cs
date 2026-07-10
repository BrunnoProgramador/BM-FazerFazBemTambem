using EM.Domain;
using EM.Domain.Validadores;
using Xunit;

namespace EM.Testes.Validadores;

public class ValidadorEncontroTestes
{
    private static Encontro EncontroValido() => new()
    {
        Turma = new Turma { Codigo = 1, Nome = "Reforço", Ano = DateTime.Today.Year },
        Data = DateTime.Today,
        Presencas = new List<PresencaAluno>
        {
            new() { Aluno = new Aluno { Matricula = 1, Nome = "João" }, Presente = true }
        }
    };

    [Fact]
    public void Encontro_valido_passa()
    {
        Assert.True(EncontroValido().Validar().Valido);
    }

    [Fact]
    public void Sem_turma_falha()
    {
        var encontro = EncontroValido();
        encontro.Turma = null!;
        Assert.False(encontro.Validar().Valido);
    }

    [Fact]
    public void Data_futura_falha()
    {
        var encontro = EncontroValido();
        encontro.Data = DateTime.Today.AddDays(1);
        Assert.False(encontro.Validar().Valido);
    }

    [Fact]
    public void Data_nao_informada_falha()
    {
        var encontro = EncontroValido();
        encontro.Data = default;
        Assert.False(encontro.Validar().Valido);
    }

    [Fact]
    public void Sem_alunos_na_lista_falha()
    {
        var encontro = EncontroValido();
        encontro.Presencas.Clear();
        Assert.False(encontro.Validar().Valido);
    }
}
