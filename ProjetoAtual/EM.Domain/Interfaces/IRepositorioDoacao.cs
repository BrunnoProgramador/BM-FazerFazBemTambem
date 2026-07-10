namespace EM.Domain.Interfaces;

public interface IRepositorioDoacao
{
    void Adicionar(Doacao entidade);
    void Atualizar(Doacao entidade);
    void Excluir(int codigo);
    IEnumerable<Doacao> ObterTodos();
    Doacao? GetByCodigo(int codigo);
}
