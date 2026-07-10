using EM.Domain.Interfaces;
using EM.Domain.Validadores;

namespace EM.Domain.Fachadas;

public class FachadaEncontro
{
    private readonly IRepositorioEncontro _repositorio;

    public FachadaEncontro(IRepositorioEncontro repositorio)
    {
        _repositorio = repositorio;
    }

    public void Registrar(Encontro encontro)
    {
        var resultado = encontro.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Adicionar(encontro);
    }

    public void Atualizar(Encontro encontro)
    {
        var resultado = encontro.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Atualizar(encontro);
    }

    public void Excluir(int codigo) => _repositorio.Excluir(codigo);

    public List<Encontro> ObterTodos() => _repositorio.ObterTodos().ToList();

    public Encontro? ObterPorCodigo(int codigo) => _repositorio.GetByCodigo(codigo);

    public List<RegistroPresencaAluno> ObterPresencasDoAluno(int matricula) =>
        _repositorio.ObterPresencasDoAluno(matricula).ToList();

    public List<ResumoPresencaAluno> ObterResumoPorAluno() =>
        _repositorio.ObterResumoPorAluno().ToList();
}
