using System.Text.RegularExpressions;
using EM.Domain.Interfaces;
using EM.Domain.Utilitarios;

namespace EM.Domain.Fachadas;

public class FachadaUsuario
{
    /// <summary>Apelidos que sempre apontam para o administrador.</summary>
    private static readonly string[] ApelidosAdmin = { "admin", "administrador" };

    private readonly IRepositorioUsuario _repositorio;

    public FachadaUsuario(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Retorna o usuário se login e senha conferem; null caso contrário.
    /// "admin" e "administrador" (em qualquer caixa) sempre resolvem
    /// para o usuário administrador do sistema.
    /// </summary>
    public Usuario? Autenticar(string? login, string? senha)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            return null;

        var normalizado = login.Trim().ToLowerInvariant();

        var usuario = _repositorio.GetByLogin(normalizado);
        if (usuario == null && ApelidosAdmin.Contains(normalizado))
            usuario = _repositorio.ObterAdministrador();

        if (usuario == null) return null;

        return GeradorHashSenha.Verificar(senha, usuario.Hash) ? usuario : null;
    }

    public bool ExisteUsuario() => _repositorio.Contar() > 0;

    public List<Usuario> ObterTodos() => _repositorio.ObterTodos().ToList();

    /// <summary>
    /// Primeiro acesso: o usuário é sempre "administrador" — a pessoa só
    /// define a senha. Funciona apenas enquanto não existir nenhum usuário.
    /// </summary>
    public Usuario CriarAdministrador(string? senha, string? confirmacao)
    {
        if (ExisteUsuario())
            throw new ArgumentException("O sistema já foi configurado. Faça login normalmente.");

        ValidarSenha(senha, confirmacao);

        var usuario = new Usuario
        {
            Login           = "administrador",
            Nome            = "Administrador",
            Hash            = GeradorHashSenha.Gerar(senha!),
            EhAdministrador = true,
            DeveTrocarSenha = false
        };

        usuario.Codigo = _repositorio.Adicionar(usuario);
        return usuario;
    }

    /// <summary>
    /// Cadastro de novo usuário pelo administrador, com senha provisória:
    /// a pessoa é obrigada a trocá-la no primeiro login.
    /// </summary>
    public Usuario CadastrarUsuario(string? login, string? nome, string? senhaProvisoria, string? confirmacao)
    {
        if (string.IsNullOrWhiteSpace(login) || login.Trim().Length < 3 || login.Trim().Length > 50)
            throw new ArgumentException("Login deve ter entre 3 e 50 caracteres.");

        var loginNormalizado = login.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(loginNormalizado, @"^[a-z0-9._-]+$"))
            throw new ArgumentException("Login deve conter apenas letras, números, ponto, hífen ou sublinhado.");

        if (ApelidosAdmin.Contains(loginNormalizado))
            throw new ArgumentException("Esse login é reservado para o administrador.");

        if (_repositorio.GetByLogin(loginNormalizado) != null)
            throw new ArgumentException($"O login \"{loginNormalizado}\" já está em uso.");

        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3 || nome.Trim().Length > 100)
            throw new ArgumentException("Nome deve ter entre 3 e 100 caracteres.");

        ValidarSenha(senhaProvisoria, confirmacao);

        var usuario = new Usuario
        {
            Login           = loginNormalizado,
            Nome            = nome.Trim(),
            Hash            = GeradorHashSenha.Gerar(senhaProvisoria!),
            EhAdministrador = false,
            DeveTrocarSenha = true
        };

        usuario.Codigo = _repositorio.Adicionar(usuario);
        return usuario;
    }

    /// <summary>Define a nova senha do próprio usuário e libera o acesso.</summary>
    public Usuario TrocarSenha(int codigo, string? novaSenha, string? confirmacao)
    {
        var usuario = _repositorio.GetByCodigo(codigo)
            ?? throw new ArgumentException("Usuário não encontrado.");

        ValidarSenha(novaSenha, confirmacao);

        if (GeradorHashSenha.Verificar(novaSenha!, usuario.Hash))
            throw new ArgumentException("A nova senha não pode ser igual à atual.");

        usuario.Hash = GeradorHashSenha.Gerar(novaSenha!);
        usuario.DeveTrocarSenha = false;
        _repositorio.Atualizar(usuario);
        return usuario;
    }

    /// <summary>Exclui um usuário. O administrador e o próprio solicitante são protegidos.</summary>
    public void ExcluirUsuario(int codigo, int codigoSolicitante)
    {
        if (codigo == codigoSolicitante)
            throw new ArgumentException("Você não pode excluir o próprio usuário.");

        var usuario = _repositorio.GetByCodigo(codigo)
            ?? throw new ArgumentException("Usuário não encontrado.");

        if (usuario.EhAdministrador)
            throw new ArgumentException("O administrador não pode ser excluído.");

        _repositorio.Excluir(codigo);
    }

    private static void ValidarSenha(string? senha, string? confirmacao)
    {
        if (string.IsNullOrEmpty(senha) || senha.Length < 8)
            throw new ArgumentException("A senha deve ter no mínimo 8 caracteres.");

        if (senha != confirmacao)
            throw new ArgumentException("A confirmação não confere com a senha.");
    }
}
