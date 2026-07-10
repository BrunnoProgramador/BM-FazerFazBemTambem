using EM.Domain;
using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class TurmaController : Controller
{
    private readonly FachadaTurma _fachadaTurma;
    private readonly ILogger<TurmaController> _logger;

    public TurmaController(FachadaTurma fachadaTurma, ILogger<TurmaController> logger)
    {
        _fachadaTurma = fachadaTurma;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var lista = _fachadaTurma.ObterTodas().ConvertAll(ConverterTurma);
        return View(lista);
    }

    [HttpGet]
    public IActionResult Edite()
    {
        return View(new TurmaModel { Ano = DateTime.Today.Year });
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var turma = _fachadaTurma.ObterPorCodigo(id);
        if (turma == null) return NotFound();
        return View("Edite", ConverterTurma(turma));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gravar(TurmaModel model, string[] dias)
    {
        try
        {
            _fachadaTurma.Adicionar(ConverterTurma(model, dias));
            TempData["Sucesso"] = $"Turma \"{model.Nome}\" criada.";
            return RedirectToAction("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edite", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Atualize(TurmaModel model, string[] dias)
    {
        try
        {
            _fachadaTurma.Atualizar(ConverterTurma(model, dias));
            TempData["Sucesso"] = $"Turma \"{model.Nome}\" atualizada.";
            return RedirectToAction("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edite", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id)
    {
        try
        {
            _fachadaTurma.Excluir(id);
            TempData["Sucesso"] = "Turma excluída.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir a turma {Codigo}.", id);
            TempData["Erro"] = "Não foi possível excluir a turma. " +
                "Ela possui encontros de frequência registrados — exclua-os antes, se realmente necessário.";
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Alunos(int id)
    {
        var turma = _fachadaTurma.ObterPorCodigo(id);
        if (turma == null) return NotFound();

        var model = ConverterTurma(turma);
        model.Alunos = _fachadaTurma.ObterAlunosDaTurma(id).ConvertAll(ConverterAluno);
        model.AlunosForaDaTurma = _fachadaTurma.ObterAlunosForaDaTurma(id).ConvertAll(ConverterAluno);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AdicionarAluno(int turmaCodigo, int matricula)
    {
        try
        {
            _fachadaTurma.AdicionarAluno(turmaCodigo, matricula);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar o aluno {Matricula} à turma {Turma}.", matricula, turmaCodigo);
            TempData["Erro"] = "Não foi possível adicionar o aluno à turma.";
        }
        return RedirectToAction("Alunos", new { id = turmaCodigo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoverAluno(int turmaCodigo, int matricula)
    {
        try
        {
            _fachadaTurma.RemoverAluno(turmaCodigo, matricula);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover o aluno {Matricula} da turma {Turma}.", matricula, turmaCodigo);
            TempData["Erro"] = "Não foi possível remover o aluno da turma.";
        }
        return RedirectToAction("Alunos", new { id = turmaCodigo });
    }

    // ── helpers ────────────────────────────────────────────────

    private static TurmaModel ConverterTurma(Turma t) => new()
    {
        Codigo      = t.Codigo,
        Nome        = t.Nome,
        Ano         = t.Ano,
        Professor   = t.Professor,
        DiasSemana  = t.DiasSemana,
        Horario     = t.Horario,
        TotalAlunos = t.TotalAlunos
    };

    private static Turma ConverterTurma(TurmaModel model, string[] dias) => new()
    {
        Codigo     = model.Codigo,
        Nome       = model.Nome,
        Ano        = model.Ano,
        Professor  = string.IsNullOrWhiteSpace(model.Professor) ? null : model.Professor.Trim(),
        DiasSemana = dias is { Length: > 0 } ? string.Join(", ", dias) : null,
        Horario    = string.IsNullOrWhiteSpace(model.Horario) ? null : model.Horario.Trim()
    };

    /// <summary>Apenas matrícula e nome — o suficiente para as listas da turma.</summary>
    private static AlunoModel ConverterAluno(Aluno a) => new()
    {
        Matricula = a.Matricula,
        Nome      = a.Nome
    };
}
