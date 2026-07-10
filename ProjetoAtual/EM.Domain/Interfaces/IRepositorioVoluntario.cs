namespace EM.Domain.Interfaces;

public interface IRepositorioVoluntario
{
    void Adicionar(Voluntario entidade);
    void Atualizar(Voluntario entidade);
    void Excluir(int codigo);
    IEnumerable<Voluntario> ObterTodos();
    Voluntario? GetByCodigo(int codigo);
}
