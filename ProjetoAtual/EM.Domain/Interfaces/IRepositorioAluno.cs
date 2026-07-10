namespace EM.Domain.Interfaces;

public interface IRepositorioAluno
{
    Aluno? GetByMatricula(int matricula);
    IEnumerable<Aluno> GetByContendoNoNome(string parteDoNome);

    /// <summary>Insere o aluno e retorna a matrícula gerada.</summary>
    int Adicionar(Aluno entidade);

    void Atualizar(Aluno entidade);
    void Excluir(int matricula);
    IEnumerable<Aluno> ObterTodos();

    void AtualizarStatus(int matricula, int statusCodigo);
    IEnumerable<Turma> ObterTurmasDoAluno(int matricula);

    IEnumerable<ObservacaoAluno> ObterObservacoes(int matricula);
    void AdicionarObservacao(ObservacaoAluno observacao);
    void ExcluirObservacao(int codigo);

    IEnumerable<HistoricoAluno> ObterHistorico(int matricula);
    void RegistrarHistorico(int matricula, string texto);
}
