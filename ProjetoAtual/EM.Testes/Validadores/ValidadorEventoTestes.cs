using EM.Domain;
using EM.Domain.Validadores;
using Xunit;

namespace EM.Testes.Validadores;

public class ValidadorEventoTestes
{
    private static Evento EventoValido() => new()
    {
        Nome = "Festa Junina",
        Data = DateTime.Today.AddDays(30)
    };

    [Fact]
    public void Evento_valido_passa()
    {
        Assert.True(EventoValido().Validar().Valido);
    }

    [Fact]
    public void Evento_futuro_eh_permitido()
    {
        var evento = EventoValido();
        evento.Data = DateTime.Today.AddMonths(6);
        Assert.True(evento.Validar().Valido);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A")]
    public void Nome_invalido_falha(string? nome)
    {
        var evento = EventoValido();
        evento.Nome = nome!;
        Assert.False(evento.Validar().Valido);
    }

    [Fact]
    public void Data_nao_informada_falha()
    {
        var evento = EventoValido();
        evento.Data = default;
        Assert.False(evento.Validar().Valido);
    }

    [Fact]
    public void Observacao_acima_de_200_caracteres_falha()
    {
        var evento = EventoValido();
        evento.Observacao = new string('o', 201);
        Assert.False(evento.Validar().Valido);
    }
}
