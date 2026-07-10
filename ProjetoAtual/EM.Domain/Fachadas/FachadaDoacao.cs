using EM.Domain.Interfaces;
using EM.Domain.Validadores;

namespace EM.Domain.Fachadas;

public class FachadaDoacao
{
    private readonly IRepositorioDoacao _repositorio;

    public FachadaDoacao(IRepositorioDoacao repositorio)
    {
        _repositorio = repositorio;
    }

    public void Adicionar(Doacao doacao)
    {
        var resultado = doacao.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Adicionar(doacao);
    }

    public void Atualizar(Doacao doacao)
    {
        var resultado = doacao.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Atualizar(doacao);
    }

    public void Excluir(int codigo) => _repositorio.Excluir(codigo);

    public List<Doacao> ObterTodos() => _repositorio.ObterTodos().ToList();

    public Doacao? ObterPorCodigo(int codigo) => _repositorio.GetByCodigo(codigo);
}
