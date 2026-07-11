namespace EM.Web.Models;

public class LoginModel
{
    public string? Login { get; set; }
    public string? Senha { get; set; }
    public bool Lembrar { get; set; }
}

/// <summary>Primeiro acesso: só define a senha do usuário "administrador".</summary>
public class SenhaAdministradorModel
{
    public string? Senha { get; set; }
    public string? Confirmacao { get; set; }
}

/// <summary>Troca de senha do próprio usuário (obrigatória quando provisória).</summary>
public class TrocarSenhaModel
{
    public string? NovaSenha { get; set; }
    public string? Confirmacao { get; set; }
}

/// <summary>Cadastro de usuário pelo administrador (senha provisória).</summary>
public class NovoUsuarioModel
{
    public string? Nome { get; set; }
    public string? Login { get; set; }
    public string? SenhaProvisoria { get; set; }
    public string? Confirmacao { get; set; }
}
