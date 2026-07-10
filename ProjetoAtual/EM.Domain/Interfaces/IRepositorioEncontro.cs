namespace EM.Domain.Interfaces;

public interface IRepositorioEncontro
{
    void Adicionar(Encontro entidade);
    void Atualizar(Encontro entidade);
    void Excluir(int codigo);

    /// <summary>Lista com turma e totais (sem a lista de presenças).</summary>
    IEnumerable<Encontro> ObterTodos();

    /// <summary>Encontro completo, com a lista de presenças.</summary>
    Encontro? GetByCodigo(int codigo);

    IEnumerable<RegistroPresencaAluno> ObterPresencasDoAluno(int matricula);
    IEnumerable<ResumoPresencaAluno> ObterResumoPorAluno();
}
