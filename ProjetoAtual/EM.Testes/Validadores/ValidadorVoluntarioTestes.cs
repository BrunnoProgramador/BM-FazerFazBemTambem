using EM.Domain;
using EM.Domain.Validadores;
using Xunit;

namespace EM.Testes.Validadores;

public class ValidadorVoluntarioTestes
{
    private static Voluntario VoluntarioValido() => new()
    {
        Nome = "Maria Auxiliadora",
        Telefone = "(62) 99999-0000",
        Funcao = "Professor"
    };

    [Fact]
    public void Voluntario_valido_passa()
    {
        Assert.True(VoluntarioValido().Validar().Valido);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("M")]
    public void Nome_invalido_falha(string? nome)
    {
        var voluntario = VoluntarioValido();
        voluntario.Nome = nome!;
        Assert.False(voluntario.Validar().Valido);
    }

    [Fact]
    public void Telefone_e_funcao_sao_opcionais()
    {
        var voluntario = VoluntarioValido();
        voluntario.Telefone = null;
        voluntario.Funcao = null;
        Assert.True(voluntario.Validar().Valido);
    }

    [Fact]
    public void Funcao_acima_de_50_caracteres_falha()
    {
        var voluntario = VoluntarioValido();
        voluntario.Funcao = new string('f', 51);
        Assert.False(voluntario.Validar().Valido);
    }
}
