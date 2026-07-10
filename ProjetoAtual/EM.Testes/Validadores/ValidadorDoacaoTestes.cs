using EM.Domain;
using EM.Domain.Validadores;
using Xunit;

namespace EM.Testes.Validadores;

public class ValidadorDoacaoTestes
{
    private static Doacao DoacaoValida() => new()
    {
        Item = "Cadernos",
        Quantidade = 50,
        Data = DateTime.Today,
        Doador = "Papelaria Central"
    };

    [Fact]
    public void Doacao_valida_passa()
    {
        Assert.True(DoacaoValida().Validar().Valido);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Item_vazio_falha(string? item)
    {
        var doacao = DoacaoValida();
        doacao.Item = item!;
        Assert.False(doacao.Validar().Valido);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Quantidade_zero_ou_negativa_falha(int quantidade)
    {
        var doacao = DoacaoValida();
        doacao.Quantidade = quantidade;
        Assert.False(doacao.Validar().Valido);
    }

    [Fact]
    public void Quantidade_nula_eh_permitida()
    {
        var doacao = DoacaoValida();
        doacao.Quantidade = null;
        Assert.True(doacao.Validar().Valido);
    }

    [Fact]
    public void Data_futura_falha()
    {
        var doacao = DoacaoValida();
        doacao.Data = DateTime.Today.AddDays(1);
        Assert.False(doacao.Validar().Valido);
    }

    [Fact]
    public void Doador_eh_opcional()
    {
        var doacao = DoacaoValida();
        doacao.Doador = null;
        Assert.True(doacao.Validar().Valido);
    }
}
