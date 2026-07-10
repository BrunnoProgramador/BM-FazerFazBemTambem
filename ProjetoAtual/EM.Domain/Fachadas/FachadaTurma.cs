using EM.Domain.Interfaces;
using EM.Domain.Validadores;

namespace EM.Domain.Fachadas;

public class FachadaTurma
{
    private readonly IRepositorioTurma _repositorio;
    private readonly IRepositorioAluno _repositorioAluno;

    public FachadaTurma(IRepositorioTurma repositorio, IRepositorioAluno repositorioAluno)
    {
        _repositorio = repositorio;
        _repositorioAluno = repositorioAluno;
    }

    public void Adicionar(Turma turma)
    {
        var resultado = turma.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Adicionar(turma);
    }

    public void Atualizar(Turma turma)
    {
        var resultado = turma.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Atualizar(turma);
    }

    public void Excluir(int codigo) => _repositorio.Excluir(codigo);

    public List<Turma> ObterTodas() => _repositorio.ObterTodos().ToList();

    public Turma? ObterPorCodigo(int codigo) => _repositorio.GetByCodigo(codigo);

    public void AdicionarAluno(int turmaCodigo, int matricula)
    {
        _repositorio.AdicionarAluno(turmaCodigo, matricula);

        var turma = _repositorio.GetByCodigo(turmaCodigo);
        if (turma != null)
            _repositorioAluno.RegistrarHistorico(matricula, $"Entrou na turma \"{turma.Nome}\".");
    }

    public void RemoverAluno(int turmaCodigo, int matricula)
    {
        _repositorio.RemoverAluno(turmaCodigo, matricula);

        var turma = _repositorio.GetByCodigo(turmaCodigo);
        if (turma != null)
            _repositorioAluno.RegistrarHistorico(matricula, $"Saiu da turma \"{turma.Nome}\".");
    }

    public List<Aluno> ObterAlunosDaTurma(int turmaCodigo) =>
        _repositorio.ObterAlunosDaTurma(turmaCodigo).ToList();

    public List<Aluno> ObterAlunosForaDaTurma(int turmaCodigo) =>
        _repositorio.ObterAlunosForaDaTurma(turmaCodigo).ToList();
}
