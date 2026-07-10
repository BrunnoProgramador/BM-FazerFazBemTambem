using EM.Domain.Interface;

namespace EM.Domain;

/// <summary>Usuário do sistema (voluntário com acesso ao FazerBemFazBemTambém).</summary>
public class Usuario : IEntidade
{
    public int Codigo { get; set; }

    /// <summary>Login em minúsculas, sem espaços.</summary>
    public string Login { get; set; }

    /// <summary>Nome de exibição.</summary>
    public string Nome { get; set; }

    /// <summary>Hash PBKDF2 da senha no formato "iterações.salt.hash" (Base64).</summary>
    public string Hash { get; set; }
}
