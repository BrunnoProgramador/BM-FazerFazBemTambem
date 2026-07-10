namespace EM.Domain.Interfaces;

public interface IRepositorioEvento
{
    void Adicionar(Evento entidade);
    void Atualizar(Evento entidade);
    void Excluir(int codigo);
    IEnumerable<Evento> ObterTodos();
    Evento? GetByCodigo(int codigo);

    void AdicionarParticipante(int eventoCodigo, int matricula);
    void RemoverParticipante(int eventoCodigo, int matricula);
    IEnumerable<Aluno> ObterParticipantes(int eventoCodigo);
    IEnumerable<Aluno> ObterNaoParticipantes(int eventoCodigo);
}
