using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Domain.Validadores;
using Xunit;

namespace EM.Testes.Validadores;

public class ValidadorAlunoTestes
{
    private static Aluno AlunoValido() => new()
    {
        Nome = "João Pedro da Silva",
        Nascimento = DateTime.Today.AddYears(-10),
        Sexo = EnumeradorSexo.Masculino
    };

    [Fact]
    public void Aluno_valido_passa()
    {
        var resultado = AlunoValido().Validar();
        Assert.True(resultado.Valido);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Nome_vazio_falha(string? nome)
    {
        var aluno = AlunoValido();
        aluno.Nome = nome!;
        Assert.False(aluno.Validar().Valido);
    }

    [Fact]
    public void Nome_com_menos_de_3_caracteres_falha()
    {
        var aluno = AlunoValido();
        aluno.Nome = "Jo";
        Assert.False(aluno.Validar().Valido);
    }

    [Fact]
    public void Nome_com_mais_de_100_caracteres_falha()
    {
        var aluno = AlunoValido();
        aluno.Nome = new string('a', 101);
        Assert.False(aluno.Validar().Valido);
    }

    [Fact]
    public void Nascimento_nao_informado_falha()
    {
        var aluno = AlunoValido();
        aluno.Nascimento = default;
        Assert.False(aluno.Validar().Valido);
    }

    [Fact]
    public void Nascimento_futuro_falha()
    {
        var aluno = AlunoValido();
        aluno.Nascimento = DateTime.Today.AddDays(1);
        Assert.False(aluno.Validar().Valido);
    }

    [Fact]
    public void Cidade_e_bairro_sao_opcionais()
    {
        var aluno = AlunoValido();
        aluno.Cidade = null;
        aluno.Bairro = null;
        Assert.True(aluno.Validar().Valido);
    }

    [Fact]
    public void Cidade_acima_de_100_caracteres_falha()
    {
        var aluno = AlunoValido();
        aluno.Cidade = new string('c', 101);
        Assert.False(aluno.Validar().Valido);
    }

    [Fact]
    public void Telefone_do_responsavel_acima_de_20_caracteres_falha()
    {
        var aluno = AlunoValido();
        aluno.ResponsavelTelefone = new string('9', 21);
        Assert.False(aluno.Validar().Valido);
    }
}
