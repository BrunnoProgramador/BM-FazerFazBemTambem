namespace EM.Domain.Interfaces;

public interface IRepositorioUsuario
{
    Usuario? GetByLogin(string login);
    Usuario? GetByCodigo(int codigo);

    /// <summary>O usuário marcado como administrador do sistema.</summary>
    Usuario? ObterAdministrador();

    int Contar();
    IEnumerable<Usuario> ObterTodos();

    /// <summary>Insere o usuário e retorna o código gerado.</summary>
    int Adicionar(Usuario entidade);

    void Atualizar(Usuario entidade);
    void Excluir(int codigo);
}
