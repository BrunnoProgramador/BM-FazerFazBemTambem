using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository;
using Xunit;

namespace EM.Testes.Integracao;

/// <summary>
/// Testes de integração contra um Firebird real (opt-in via EM_TESTE_FIREBIRD).
/// Use um banco de TESTE: os testes criam e excluem os próprios registros,
/// mas rodar contra o banco de produção nunca é boa ideia.
/// </summary>
public class BancoTestes
{
    private static string ConnectionString => FatoIntegracaoAttribute.ConnectionString!;

    [FatoIntegracao]
    public void Migrador_eh_idempotente_rodando_duas_vezes()
    {
        MigradorBanco.Executar(ConnectionString);
        MigradorBanco.Executar(ConnectionString); // segunda execução não pode falhar
    }

    [FatoIntegracao]
    public void Aluno_faz_o_ciclo_completo_de_crud()
    {
        MigradorBanco.Executar(ConnectionString);
        var repositorio = new RepositorioAluno(ConnectionString);

        // cria
        var matricula = repositorio.Adicionar(new Aluno
        {
            Nome = $"Teste Integração {Guid.NewGuid():N}",
            Nascimento = DateTime.Today.AddYears(-8),
            Sexo = EnumeradorSexo.Feminino,
            Bairro = "Bairro do Teste"
        });
        Assert.True(matricula > 0);

        try
        {
            // lê
            var aluno = repositorio.GetByMatricula(matricula);
            Assert.NotNull(aluno);
            Assert.Equal("Bairro do Teste", aluno!.Bairro);
            Assert.Equal(EnumeradorStatusAluno.Ativo.Codigo, aluno.Status.Codigo);

            // atualiza
            aluno.Nome = "Nome Atualizado Pelo Teste";
            aluno.Status = EnumeradorStatusAluno.Inativo;
            repositorio.Atualizar(aluno);

            var atualizado = repositorio.GetByMatricula(matricula);
            Assert.Equal("Nome Atualizado Pelo Teste", atualizado!.Nome);
            Assert.Equal(EnumeradorStatusAluno.Inativo.Codigo, atualizado.Status.Codigo);

            // histórico e observações
            repositorio.RegistrarHistorico(matricula, "Registro criado pelo teste de integração.");
            Assert.Single(repositorio.ObterHistorico(matricula));

            repositorio.AdicionarObservacao(new ObservacaoAluno
            {
                AlunoMatricula = matricula,
                Data = DateTime.Today,
                Texto = "Observação do teste."
            });
            Assert.Single(repositorio.ObterObservacoes(matricula));
        }
        finally
        {
            // exclui (limpa observações, histórico e vínculos em transação)
            repositorio.Excluir(matricula);
        }

        Assert.Null(repositorio.GetByMatricula(matricula));
    }

    [FatoIntegracao]
    public void Usuario_grava_e_autentica_no_banco_real()
    {
        MigradorBanco.Executar(ConnectionString);
        var repositorio = new RepositorioUsuario(ConnectionString);

        var login = $"teste_{Guid.NewGuid():N}"[..20];
        var codigo = repositorio.Adicionar(new Usuario
        {
            Login = login,
            Nome = "Usuário do Teste",
            Hash = GeradorHashSenha.Gerar("senhaDoTeste123")
        });

        try
        {
            var usuario = repositorio.GetByLogin(login);
            Assert.NotNull(usuario);
            Assert.True(GeradorHashSenha.Verificar("senhaDoTeste123", usuario!.Hash));
            Assert.False(GeradorHashSenha.Verificar("senhaErrada", usuario.Hash));
        }
        finally
        {
            repositorio.Excluir(codigo);
        }
    }
}
