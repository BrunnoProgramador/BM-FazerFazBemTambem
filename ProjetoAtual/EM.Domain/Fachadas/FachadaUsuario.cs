using System.Text.RegularExpressions;
using EM.Domain.Interfaces;
using EM.Domain.Utilitarios;

namespace EM.Domain.Fachadas;

public class FachadaUsuario
{
    private readonly IRepositorioUsuario _repositorio;

    public FachadaUsuario(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>Retorna o usuário se login e senha conferem; null caso contrário.</summary>
    public Usuario? Autenticar(string? login, string? senha)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            return null;

        var usuario = _repositorio.GetByLogin(login.Trim().ToLowerInvariant());
        if (usuario == null) return null;

        return GeradorHashSenha.Verificar(senha, usuario.Hash) ? usuario : null;
    }

    public bool ExisteUsuario() => _repositorio.Contar() > 0;

    /// <summary>
    /// Cria o primeiro usuário do sistema (tela de configuração inicial).
    /// Só funciona enquanto não existir nenhum usuário cadastrado.
    /// </summary>
    public Usuario CriarPrimeiroUsuario(string? login, string? nome, string? senha, string? confirmacao)
    {
        if (ExisteUsuario())
            throw new ArgumentException("O sistema já foi configurado. Faça login normalmente.");

        if (string.IsNullOrWhiteSpace(login) || login.Trim().Length < 3 || login.Trim().Length > 50)
            throw new ArgumentException("Login deve ter entre 3 e 50 caracteres.");

        var loginNormalizado = login.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(loginNormalizado, @"^[a-z0-9._-]+$"))
            throw new ArgumentException("Login deve conter apenas letras, números, ponto, hífen ou sublinhado.");

        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3 || nome.Trim().Length > 100)
            throw new ArgumentException("Nome deve ter entre 3 e 100 caracteres.");

        if (string.IsNullOrEmpty(senha) || senha.Length < 8)
            throw new ArgumentException("A senha deve ter no mínimo 8 caracteres.");

        if (senha != confirmacao)
            throw new ArgumentException("A confirmação não confere com a senha.");

        var usuario = new Usuario
        {
            Login = loginNormalizado,
            Nome  = nome.Trim(),
            Hash  = GeradorHashSenha.Gerar(senha)
        };

        usuario.Codigo = _repositorio.Adicionar(usuario);
        return usuario;
    }
}
