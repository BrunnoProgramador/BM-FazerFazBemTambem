using EM.Domain.Interfaces;
using EM.Domain.Utilitarios;
using EM.Domain.Validadores;

namespace EM.Domain.Fachadas;

public class FachadaAluno
{
    private readonly IRepositorioAluno _repositorioAluno;

    public FachadaAluno(IRepositorioAluno repositorioAluno)
    {
        _repositorioAluno = repositorioAluno;
    }

    public int AdicionarAluno(Aluno aluno)
    {
        var resultado = aluno.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);

        var matricula = _repositorioAluno.Adicionar(aluno);
        _repositorioAluno.RegistrarHistorico(matricula, "Cadastrado no projeto.");
        return matricula;
    }

    public void AtualizeAluno(Aluno entidade)
    {
        var resultado = entidade.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);

        var anterior = _repositorioAluno.GetByMatricula(entidade.Matricula)
            ?? throw new ArgumentException("Aluno não encontrado.");

        _repositorioAluno.Atualizar(entidade);

        if (anterior.Status.Codigo != entidade.Status.Codigo)
            _repositorioAluno.RegistrarHistorico(entidade.Matricula,
                $"Status alterado para \"{entidade.Status.Descricao}\".");
    }

    public void ExcluirAluno(int matricula)
    {
        _repositorioAluno.Excluir(matricula);
    }

    public void AlterarStatus(int matricula, int statusCodigo)
    {
        var aluno = _repositorioAluno.GetByMatricula(matricula)
            ?? throw new ArgumentException("Aluno não encontrado.");

        var novoStatus = EnumeradorStatusAluno.ObtenhaPorCodigo(statusCodigo);
        if (aluno.Status.Codigo == novoStatus.Codigo) return;

        _repositorioAluno.AtualizarStatus(matricula, novoStatus.Codigo);
        _repositorioAluno.RegistrarHistorico(matricula, $"Status alterado para \"{novoStatus.Descricao}\".");
    }

    public Aluno? ObtenhaPorMatricula(int matricula)
    {
        return _repositorioAluno.GetByMatricula(matricula);
    }

    public List<Aluno> ObterTodosAlunos()
    {
        return _repositorioAluno.ObterTodos().ToList();
    }

    public List<Aluno> ObtenhaPorNome(string parteDoNome)
    {
        return _repositorioAluno.GetByContendoNoNome(parteDoNome).ToList();
    }

    public List<Turma> ObterTurmasDoAluno(int matricula)
    {
        return _repositorioAluno.ObterTurmasDoAluno(matricula).ToList();
    }

    // ── observações ────────────────────────────────────────────

    public List<ObservacaoAluno> ObterObservacoes(int matricula)
    {
        return _repositorioAluno.ObterObservacoes(matricula).ToList();
    }

    public void AdicionarObservacao(int matricula, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Escreva a observação antes de salvar.");

        if (texto.Trim().Length > 200)
            throw new ArgumentException("Observação deve ter no máximo 200 caracteres.");

        _repositorioAluno.AdicionarObservacao(new ObservacaoAluno
        {
            AlunoMatricula = matricula,
            Data = DateTime.Today,
            Texto = texto.Trim()
        });
    }

    public void ExcluirObservacao(int codigo)
    {
        _repositorioAluno.ExcluirObservacao(codigo);
    }

    // ── histórico ──────────────────────────────────────────────

    public List<HistoricoAluno> ObterHistorico(int matricula)
    {
        return _repositorioAluno.ObterHistorico(matricula).ToList();
    }
}
