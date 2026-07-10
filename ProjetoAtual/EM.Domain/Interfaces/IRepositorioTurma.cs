namespace EM.Domain.Interfaces;

public interface IRepositorioTurma
{
    void Adicionar(Turma entidade);
    void Atualizar(Turma entidade);
    void Excluir(int codigo);
    IEnumerable<Turma> ObterTodos();
    Turma? GetByCodigo(int codigo);
    void AdicionarAluno(int turmaCodigo, int matricula);
    void RemoverAluno(int turmaCodigo, int matricula);
    IEnumerable<Aluno> ObterAlunosDaTurma(int turmaCodigo);
    IEnumerable<Aluno> ObterAlunosForaDaTurma(int turmaCodigo);
}
