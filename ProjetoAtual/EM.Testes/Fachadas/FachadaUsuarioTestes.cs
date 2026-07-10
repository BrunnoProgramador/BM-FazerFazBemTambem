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

    // ── primeiro usuário ───────────────────────────────────────

    [Fact]
    public void Cria_primeiro_usuario_com_login_normalizado()
    {
        var usuario = _fachada.CriarPrimeiroUsuario("  Admin ", "Brunno Santos", "senhaForte1", "senhaForte1");

        Assert.Equal("admin", usuario.Login);
        Assert.Equal("Brunno Santos", usuario.Nome);
        Assert.True(usuario.Codigo > 0);
        Assert.True(_fachada.ExisteUsuario());
    }

    [Fact]
    public void Nao_cria_segundo_usuario_pela_configuracao_inicial()
    {
        _fachada.CriarPrimeiroUsuario("admin", "Administrador", "senhaForte1", "senhaForte1");

        Assert.Throws<ArgumentException>(() =>
            _fachada.CriarPrimeiroUsuario("outro", "Outro", "senhaForte1", "senhaForte1"));
    }

    [Theory]
    [InlineData("ab")]                     // login curto
    [InlineData("login com espaço")]       // caractere inválido
    [InlineData("usuário")]                // acento
    public void Login_invalido_falha(string login)
    {
        Assert.Throws<ArgumentException>(() =>
            _fachada.CriarPrimeiroUsuario(login, "Nome Completo", "senhaForte1", "senhaForte1"));
    }

    [Fact]
    public void Senha_curta_falha()
    {
        Assert.Throws<ArgumentException>(() =>
            _fachada.CriarPrimeiroUsuario("admin", "Administrador", "1234567", "1234567"));
    }

    [Fact]
    public void Confirmacao_diferente_falha()
    {
        Assert.Throws<ArgumentException>(() =>
            _fachada.CriarPrimeiroUsuario("admin", "Administrador", "senhaForte1", "senhaForte2"));
    }

    // ── autenticação ───────────────────────────────────────────

    [Fact]
    public void Autentica_com_credenciais_corretas()
    {
        _fachada.CriarPrimeiroUsuario("admin", "Administrador", "senhaForte1", "senhaForte1");

        var usuario = _fachada.Autenticar("ADMIN", "senhaForte1"); // login com caixa diferente
        Assert.NotNull(usuario);
        Assert.Equal("admin", usuario!.Login);
    }

    [Fact]
    public void Senha_errada_retorna_null()
    {
        _fachada.CriarPrimeiroUsuario("admin", "Administrador", "senhaForte1", "senhaForte1");
        Assert.Null(_fachada.Autenticar("admin", "senhaErrada"));
    }

    [Fact]
    public void Usuario_inexistente_retorna_null()
    {
        Assert.Null(_fachada.Autenticar("ninguem", "qualquer"));
    }

    [Theory]
    [InlineData(null, "senha")]
    [InlineData("login", null)]
    [InlineData("", "")]
    public void Credenciais_vazias_retornam_null(string? login, string? senha)
    {
        Assert.Null(_fachada.Autenticar(login, senha));
    }
}

/// <summary>Repositório em memória para testes da fachada.</summary>
internal class FakeRepositorioUsuario : IRepositorioUsuario
{
    private readonly List<Usuario> _usuarios = new();
    private int _proximoCodigo = 1;

    public Usuario? GetByLogin(string login) =>
        _usuarios.FirstOrDefault(u => u.Login == login);

    public int Contar() => _usuarios.Count;

    public int Adicionar(Usuario entidade)
    {
        entidade.Codigo = _proximoCodigo++;
        _usuarios.Add(entidade);
        return entidade.Codigo;
    }
}
