namespace EM.Web.Models;

public class LoginModel
{
    public string? Login { get; set; }
    public string? Senha { get; set; }
    public bool Lembrar { get; set; }
}

/// <summary>Criação do primeiro usuário (configuração inicial do sistema).</summary>
public class PrimeiroUsuarioModel
{
    public string? Login { get; set; }
    public string? Nome { get; set; }
    public string? Senha { get; set; }
    public string? ConfirmacaoSenha { get; set; }
}
