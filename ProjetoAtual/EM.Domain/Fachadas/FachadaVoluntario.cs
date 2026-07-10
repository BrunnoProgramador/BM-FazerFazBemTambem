using EM.Domain.Interfaces;
using EM.Domain.Validadores;

namespace EM.Domain.Fachadas;

public class FachadaVoluntario
{
    private readonly IRepositorioVoluntario _repositorio;

    public FachadaVoluntario(IRepositorioVoluntario repositorio)
    {
        _repositorio = repositorio;
    }

    public void Adicionar(Voluntario voluntario)
    {
        var resultado = voluntario.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Adicionar(voluntario);
    }

    public void Atualizar(Voluntario voluntario)
    {
        var resultado = voluntario.Validar();
        if (!resultado.Valido) throw new ArgumentException(resultado.Mensagem);
        _repositorio.Atualizar(voluntario);
    }

    public void Excluir(int codigo) => _repositorio.Excluir(codigo);

    public List<Voluntario> ObterTodos() => _repositorio.ObterTodos().ToList();

    public Voluntario? ObterPorCodigo(int codigo) => _repositorio.GetByCodigo(codigo);
}
