using EM.Domain.Interfaces;
using EM.Domain.Validadores;

namespace EM.Domain.Fachadas;

public class FachadaEvento
{
    private readonly IRepositorioEvento _repositorio;
    private readonly IRepositorioAluno _repositorioAluno;

    public FachadaEvento(IRepositorioEvento repositorio, IRepositorioAluno repositorioAluno)
    {
        _repositorio = repositorio;
        _repositorioAluno = repositorioAluno;
    }

    public void Adicionar(Evento evento)
    {
        var resultado = evento.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Adicionar(evento);
    }

    public void Atualizar(Evento evento)
    {
        var resultado = evento.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Atualizar(evento);
    }

    public void Excluir(int codigo) => _repositorio.Excluir(codigo);

    public List<Evento> ObterTodos() => _repositorio.ObterTodos().ToList();

    public Evento? ObterPorCodigo(int codigo) => _repositorio.GetByCodigo(codigo);

    public void AdicionarParticipante(int eventoCodigo, int matricula)
    {
        _repositorio.AdicionarParticipante(eventoCodigo, matricula);

        var evento = _repositorio.GetByCodigo(eventoCodigo);
        if (evento != null)
            _repositorioAluno.RegistrarHistorico(matricula, $"Participou do evento \"{evento.Nome}\".");
    }

    public void RemoverParticipante(int eventoCodigo, int matricula) =>
        _repositorio.RemoverParticipante(eventoCodigo, matricula);

    public List<Aluno> ObterParticipantes(int eventoCodigo) =>
        _repositorio.ObterParticipantes(eventoCodigo).ToList();

    public List<Aluno> ObterNaoParticipantes(int eventoCodigo) =>
        _repositorio.ObterNaoParticipantes(eventoCodigo).ToList();
}
