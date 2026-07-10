namespace EM.Domain.Interfaces;

public interface IRepositorioUsuario
{
    Usuario? GetByLogin(string login);
    int Contar();

    /// <summary>Insere o usuário e retorna o código gerado.</summary>
    int Adicionar(Usuario entidade);
}
