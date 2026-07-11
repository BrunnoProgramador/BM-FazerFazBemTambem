using EM.Domain;
using EM.Domain.Fachadas;
using EM.Domain.Interfaces;
using Xunit;

namespace EM.Testes.Fachadas;

public class FachadaUsuarioTestes
{
    private readonly FakeRepositorioUsuario _repositorio = new();
    private readonly FachadaUsuario _fachada;

    public FachadaUsuarioTestes()
    {
        _fachada = new FachadaUsuario(_repositorio);
    }

    // ── primeiro acesso (administrador) ────────────────────────

    [Fact]
    public void Primeiro_acesso_cria_o_administrador_definindo_so_a_senha()
    {
        var usuario = _fachada.CriarAdministrador("senhaForte1", "senhaForte1");

        Assert.Equal("administrador", usuario.Login);
        Assert.True(usuario.EhAdministrador);
        Assert.False(usuario.DeveTrocarSenha);
        Assert.True(_fachada.ExisteUsuario());
    }

    [Fact]
    public void Nao_cria_administrador_duas_vezes()
    {
        _fachada.CriarAdministrador("senhaForte1", "senhaForte1");

        Assert.Throws<ArgumentException>(() =>
            _fachada.CriarAdministrador("outraSenha1", "outraSenha1"));
    }

    [Fact]
    public void Senha_curta_ou_confirmacao_errada_falham()
    {
        Assert.Throws<ArgumentException>(() => _fachada.CriarAdministrador("1234567", "1234567"));
        Assert.Throws<ArgumentException>(() => _fachada.CriarAdministrador("senhaForte1", "senhaForte2"));
    }

    // ── autenticação (apelidos do admin) ───────────────────────

    [Theory]
    [InlineData("administrador")]
    [InlineData("ADMINISTRADOR")]
    [InlineData("admin")]
    [InlineData("Admin")]
    [InlineData("  admin  ")]
    public void Admin_entra_escrevendo_o_login_de_qualquer_jeito(string login)
    {
        _fachada.CriarAdministrador("senhaForte1", "senhaForte1");

        var usuario = _fachada.Autenticar(login, "senhaForte1");
        Assert.NotNull(usuario);
        Assert.True(usuario!.EhAdministrador);
    }

    [Fact]
    public void Senha_errada_retorna_null()
    {
        _fachada.CriarAdministrador("senhaForte1", "senhaForte1");
        Assert.Null(_fachada.Autenticar("admin", "senhaErrada"));
    }

    // ── cadastro de usuários pelo admin ────────────────────────

    [Fact]
    public void Usuario_cadastrado_nasce_com_troca_de_senha_pendente()
    {
        var usuario = _fachada.CadastrarUsuario("maria", "Maria Silva", "provisoria1", "provisoria1");

        Assert.True(usuario.DeveTrocarSenha);
        Assert.False(usuario.EhAdministrador);
        Assert.NotNull(_fachada.Autenticar("maria", "provisoria1"));
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("administrador")]
    public void Logins_reservados_do_admin_sao_rejeitados(string login)
    {
        Assert.Throws<ArgumentException>(() =>
            _fachada.CadastrarUsuario(login, "Fulano", "provisoria1", "provisoria1"));
    }

    [Fact]
    public void Login_duplicado_eh_rejeitado()
    {
        _fachada.CadastrarUsuario("maria", "Maria Silva", "provisoria1", "provisoria1");

        Assert.Throws<ArgumentException>(() =>
            _fachada.CadastrarUsuario("MARIA", "Outra Maria", "provisoria1", "provisoria1"));
    }

    // ── troca de senha ─────────────────────────────────────────

    [Fact]
    public void Trocar_senha_libera_o_acesso_e_invalida_a_provisoria()
    {
        var usuario = _fachada.CadastrarUsuario("joao", "João Souza", "provisoria1", "provisoria1");

        var atualizado = _fachada.TrocarSenha(usuario.Codigo, "definitiva1", "definitiva1");

        Assert.False(atualizado.DeveTrocarSenha);
        Assert.NotNull(_fachada.Autenticar("joao", "definitiva1"));
        Assert.Null(_fachada.Autenticar("joao", "provisoria1"));
    }

    [Fact]
    public void Nova_senha_igual_a_provisoria_eh_rejeitada()
    {
        var usuario = _fachada.CadastrarUsuario("joao", "João Souza", "provisoria1", "provisoria1");

        Assert.Throws<ArgumentException>(() =>
            _fachada.TrocarSenha(usuario.Codigo, "provisoria1", "provisoria1"));
    }

    // ── exclusão ───────────────────────────────────────────────

    [Fact]
    public void Administrador_nao_pode_ser_excluido()
    {
        var admin = _fachada.CriarAdministrador("senhaForte1", "senhaForte1");
        var outro = _fachada.CadastrarUsuario("maria", "Maria Silva", "provisoria1", "provisoria1");

        Assert.Throws<ArgumentException>(() => _fachada.ExcluirUsuario(admin.Codigo, outro.Codigo));
    }

    [Fact]
    public void Ninguem_exclui_a_si_mesmo()
    {
        var admin = _fachada.CriarAdministrador("senhaForte1", "senhaForte1");
        Assert.Throws<ArgumentException>(() => _fachada.ExcluirUsuario(admin.Codigo, admin.Codigo));
    }

    [Fact]
    public void Admin_exclui_usuario_comum()
    {
        var admin = _fachada.CriarAdministrador("senhaForte1", "senhaForte1");
        var outro = _fachada.CadastrarUsuario("maria", "Maria Silva", "provisoria1", "provisoria1");

        _fachada.ExcluirUsuario(outro.Codigo, admin.Codigo);

        Assert.Null(_fachada.Autenticar("maria", "provisoria1"));
    }
}

/// <summary>Repositório em memória para testes da fachada.</summary>
internal class FakeRepositorioUsuario : IRepositorioUsuario
{
    private readonly List<Usuario> _usuarios = new();
    private int _proximoCodigo = 1;

    public Usuario? GetByLogin(string login) =>
        _usuarios.FirstOrDefault(u => u.Login == login);

    public Usuario? GetByCodigo(int codigo) =>
        _usuarios.FirstOrDefault(u => u.Codigo == codigo);

    public Usuario? ObterAdministrador() =>
        _usuarios.FirstOrDefault(u => u.EhAdministrador);

    public int Contar() => _usuarios.Count;

    public IEnumerable<Usuario> ObterTodos() => _usuarios.OrderBy(u => u.Nome);

    public int Adicionar(Usuario entidade)
    {
        entidade.Codigo = _proximoCodigo++;
        _usuarios.Add(entidade);
        return entidade.Codigo;
    }

    public void Atualizar(Usuario entidade)
    {
        var indice = _usuarios.FindIndex(u => u.Codigo == entidade.Codigo);
        if (indice >= 0) _usuarios[indice] = entidade;
    }

    public void Excluir(int codigo) =>
        _usuarios.RemoveAll(u => u.Codigo == codigo);
}
