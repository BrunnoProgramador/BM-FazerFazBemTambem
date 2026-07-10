using EM.Domain;
using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class EventoController : Controller
{
    private readonly FachadaEvento _fachada;
    private readonly ILogger<EventoController> _logger;

    public EventoController(FachadaEvento fachada, ILogger<EventoController> logger)
    {
        _fachada = fachada;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var lista = _fachada.ObterTodos().ConvertAll(Converter);
        return View(lista);
    }

    [HttpGet]
    public IActionResult Edite()
    {
        return View(new EventoModel { Data = DateTime.Today });
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var evento = _fachada.ObterPorCodigo(id);
        if (evento == null) return NotFound();
        return View("Edite", Converter(evento));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gravar(EventoModel model)
    {
        try
        {
            _fachada.Adicionar(ConverterEvento(model));
            TempData["Sucesso"] = $"Evento \"{model.Nome}\" criado.";
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
    public IActionResult Atualize(EventoModel model)
    {
        try
        {
            _fachada.Atualizar(ConverterEvento(model));
            TempData["Sucesso"] = $"Evento \"{model.Nome}\" atualizado.";
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
            _fachada.Excluir(id);
            TempData["Sucesso"] = "Evento excluído.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir o evento {Codigo}.", id);
            TempData["Erro"] = "Não foi possível excluir o evento.";
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Participantes(int id)
    {
        var evento = _fachada.ObterPorCodigo(id);
        if (evento == null) return NotFound();

        var model = Converter(evento);
        model.Participantes = _fachada.ObterParticipantes(id).ConvertAll(ConverterAluno);
        model.Disponiveis = _fachada.ObterNaoParticipantes(id).ConvertAll(ConverterAluno);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AdicionarParticipante(int eventoCodigo, int matricula)
    {
        try
        {
            _fachada.AdicionarParticipante(eventoCodigo, matricula);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar o aluno {Matricula} ao evento {Evento}.", matricula, eventoCodigo);
            TempData["Erro"] = "Não foi possível adicionar o participante.";
        }
        return RedirectToAction("Participantes", new { id = eventoCodigo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoverParticipante(int eventoCodigo, int matricula)
    {
        try
        {
            _fachada.RemoverParticipante(eventoCodigo, matricula);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover o aluno {Matricula} do evento {Evento}.", matricula, eventoCodigo);
            TempData["Erro"] = "Não foi possível remover o participante.";
        }
        return RedirectToAction("Participantes", new { id = eventoCodigo });
    }

    // ── helpers ────────────────────────────────────────────────

    private static EventoModel Converter(Evento e) => new()
    {
        Codigo             = e.Codigo,
        Nome               = e.Nome,
        Data               = e.Data,
        Observacao         = e.Observacao,
        TotalParticipantes = e.TotalParticipantes
    };

    private static Evento ConverterEvento(EventoModel model) => new()
    {
        Codigo     = model.Codigo,
        Nome       = model.Nome,
        Data       = model.Data,
        Observacao = string.IsNullOrWhiteSpace(model.Observacao) ? null : model.Observacao.Trim()
    };

    private static AlunoModel ConverterAluno(Aluno a) => new()
    {
        Matricula = a.Matricula,
        Nome      = a.Nome
    };
}
