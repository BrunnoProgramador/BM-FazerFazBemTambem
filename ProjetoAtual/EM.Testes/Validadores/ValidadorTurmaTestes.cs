using EM.Domain;
using EM.Domain.Validadores;
using Xunit;

namespace EM.Testes.Validadores;

public class ValidadorTurmaTestes
{
    private static Turma TurmaValida() => new()
    {
        Nome = "6 a 8 anos",
        Ano = DateTime.Today.Year
    };

    [Fact]
    public void Turma_valida_passa()
    {
        Assert.True(TurmaValida().Validar().Valido);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A")]
    public void Nome_invalido_falha(string? nome)
    {
        var turma = TurmaValida();
        turma.Nome = nome!;
        Assert.False(turma.Validar().Valido);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    [InlineData(0)]
    public void Ano_fora_da_faixa_falha(int ano)
    {
        var turma = TurmaValida();
        turma.Ano = ano;
        Assert.False(turma.Validar().Valido);
    }

    [Fact]
    public void Professor_dias_e_horario_sao_opcionais()
    {
        var turma = TurmaValida();
        turma.Professor = null;
        turma.DiasSemana = null;
        turma.Horario = null;
        Assert.True(turma.Validar().Valido);
    }

    [Fact]
    public void Todos_os_sete_dias_da_semana_cabem_no_limite()
    {
        var turma = TurmaValida();
        turma.DiasSemana = "Segunda, Terça, Quarta, Quinta, Sexta, Sábado, Domingo";
        Assert.True(turma.Validar().Valido);
    }

    [Fact]
    public void Professor_acima_de_100_caracteres_falha()
    {
        var turma = TurmaValida();
        turma.Professor = new string('p', 101);
        Assert.False(turma.Validar().Valido);
    }
}
