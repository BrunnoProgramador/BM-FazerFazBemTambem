using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class RelatorioController : Controller
{
    private readonly FachadaTurma _fachadaTurma;
    private readonly FachadaEncontro _fachadaEncontro;

    public RelatorioController(FachadaTurma fachadaTurma, FachadaEncontro fachadaEncontro)
    {
        _fachadaTurma = fachadaTurma;
        _fachadaEncontro = fachadaEncontro;
    }

    public IActionResult Index()
    {
        var encontros = _fachadaEncontro.ObterTodos();
        var porTurma = encontros.GroupBy(e => e.Turma.Codigo)
            .ToDictionary(g => g.Key, g => g.ToList());

        var turmas = _fachadaTurma.ObterTodas().ConvertAll(t =>
        {
            porTurma.TryGetValue(t.Codigo, out var encontrosDaTurma);
            encontrosDaTurma ??= new();

            var somaAlunos = encontrosDaTurma.Sum(e => e.TotalAlunos);
            var somaPresentes = encontrosDaTurma.Sum(e => e.TotalPresentes);

            return new RelatorioTurmaModel
            {
                Codigo          = t.Codigo,
                Nome            = t.Nome,
                TotalAlunos     = t.TotalAlunos,
                Encontros       = encontrosDaTurma.Count,
                UltimoEncontro  = encontrosDaTurma.Count > 0
                    ? encontrosDaTurma.Max(e => e.Data)
                    : null,
                PercentualMedio = somaAlunos == 0
                    ? 0
                    : (int)Math.Round(100.0 * somaPresentes / somaAlunos)
            };
        });

        var model = new RelatorioModel
        {
            Turmas = turmas,
            Alunos = _fachadaEncontro.ObterResumoPorAluno()
        };

        return View(model);
    }
}
