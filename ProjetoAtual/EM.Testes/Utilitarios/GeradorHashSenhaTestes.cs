using EM.Domain.Utilitarios;
using Xunit;

namespace EM.Testes.Utilitarios;

public class GeradorHashSenhaTestes
{
    [Fact]
    public void Senha_correta_verifica_com_sucesso()
    {
        var hash = GeradorHashSenha.Gerar("minhaSenhaSegura123");
        Assert.True(GeradorHashSenha.Verificar("minhaSenhaSegura123", hash));
    }

    [Fact]
    public void Senha_errada_falha_na_verificacao()
    {
        var hash = GeradorHashSenha.Gerar("minhaSenhaSegura123");
        Assert.False(GeradorHashSenha.Verificar("outraSenha", hash));
    }

    [Fact]
    public void Mesma_senha_gera_hashes_diferentes_por_causa_do_salt()
    {
        var hash1 = GeradorHashSenha.Gerar("mesmaSenha");
        var hash2 = GeradorHashSenha.Gerar("mesmaSenha");
        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("texto-sem-formato")]
    [InlineData("a.b")]
    [InlineData("abc.###.###")]
    public void Hash_malformado_nao_lanca_excecao_e_retorna_falso(string hashInvalido)
    {
        Assert.False(GeradorHashSenha.Verificar("qualquer", hashInvalido));
    }

    [Fact]
    public void Hash_tem_formato_iteracoes_salt_hash()
    {
        var hash = GeradorHashSenha.Gerar("senha12345");
        var partes = hash.Split('.');
        Assert.Equal(3, partes.Length);
        Assert.True(int.Parse(partes[0]) >= 100_000);
    }
}
