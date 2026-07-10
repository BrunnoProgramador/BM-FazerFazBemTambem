using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class DashboardController : Controller
{
    private readonly FachadaAluno _fachadaAluno;
    private readonly FachadaTurma _fachadaTurma;
    private readonly FachadaEncontro _fachadaEncontro;

    public DashboardController(
        FachadaAluno fachadaAluno,
        FachadaTurma fachadaTurma,
        FachadaEncontro fachadaEncontro)
    {
        _fachadaAluno = fachadaAluno;
        _fachadaTurma = fachadaTurma;
        _fachadaEncontro = fachadaEncontro;
    }

    public IActionResult Index()
    {
        var encontros = _fachadaEncontro.ObterTodos();
        var somaAlunos = encontros.Sum(e => e.TotalAlunos);
        var somaPresentes = encontros.Sum(e => e.TotalPresentes);

        var model = new DashboardModel
        {
            TotalAlunosAtivos = _fachadaAluno.ObterTodosAlunos().Count(a => a.Status.Codigo == 0),
            TotalTurmas = _fachadaTurma.ObterTodas().Count,
            EncontrosRealizados = encontros.Count,
            PresencaMedia = somaAlunos == 0
                ? null
                : (int)Math.Round(100.0 * somaPresentes / somaAlunos),
            EncontrosHoje = encontros
                .Where(e => e.Data.Date == DateTime.Today)
                .Select(e => new EncontroModel
                {
                    Codigo         = e.Codigo,
                    TurmaNome      = e.Turma.Nome,
                    Data           = e.Data,
                    TotalAlunos    = e.TotalAlunos,
                    TotalPresentes = e.TotalPresentes
                })
                .ToList()
        };

        return View(model);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
