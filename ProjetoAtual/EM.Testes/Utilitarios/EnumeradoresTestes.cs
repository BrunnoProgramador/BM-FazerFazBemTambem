using EM.Domain.Utilitarios;
using Xunit;

namespace EM.Testes.Utilitarios;

public class EnumeradoresTestes
{
    [Fact]
    public void Sexo_por_codigo_retorna_masculino_e_feminino()
    {
        Assert.Equal(EnumeradorSexo.Masculino, EnumeradorSexo.ObtenhaPorCodigo(0));
        Assert.Equal(EnumeradorSexo.Feminino, EnumeradorSexo.ObtenhaPorCodigo(1));
    }

    [Fact]
    public void Status_por_codigo_retorna_o_status_correto()
    {
        Assert.Equal(EnumeradorStatusAluno.Ativo, EnumeradorStatusAluno.ObtenhaPorCodigo(0));
        Assert.Equal(EnumeradorStatusAluno.Inativo, EnumeradorStatusAluno.ObtenhaPorCodigo(1));
        Assert.Equal(EnumeradorStatusAluno.MudouDeCidade, EnumeradorStatusAluno.ObtenhaPorCodigo(2));
        Assert.Equal(EnumeradorStatusAluno.ParouDeFrequentar, EnumeradorStatusAluno.ObtenhaPorCodigo(3));
    }

    [Fact]
    public void Status_com_codigo_desconhecido_cai_em_ativo()
    {
        Assert.Equal(EnumeradorStatusAluno.Ativo, EnumeradorStatusAluno.ObtenhaPorCodigo(99));
    }

    [Fact]
    public void Todos_os_status_tem_descricao()
    {
        Assert.All(EnumeradorStatusAluno.Todos,
            s => Assert.False(string.IsNullOrWhiteSpace(s.Descricao)));
    }
}
