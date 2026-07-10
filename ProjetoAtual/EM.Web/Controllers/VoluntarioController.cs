using EM.Domain;
using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class VoluntarioController : Controller
{
    private readonly FachadaVoluntario _fachada;
    private readonly ILogger<VoluntarioController> _logger;

    public VoluntarioController(FachadaVoluntario fachada, ILogger<VoluntarioController> logger)
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
        return View(new VoluntarioModel());
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var voluntario = _fachada.ObterPorCodigo(id);
        if (voluntario == null) return NotFound();
        return View("Edite", Converter(voluntario));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gravar(VoluntarioModel model)
    {
        try
        {
            _fachada.Adicionar(ConverterVoluntario(model));
            TempData["Sucesso"] = $"Voluntário \"{model.Nome}\" cadastrado.";
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
    public IActionResult Atualize(VoluntarioModel model)
    {
        try
        {
            _fachada.Atualizar(ConverterVoluntario(model));
            TempData["Sucesso"] = $"Voluntário \"{model.Nome}\" atualizado.";
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
            TempData["Sucesso"] = "Voluntário excluído.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir o voluntário {Codigo}.", id);
            TempData["Erro"] = "Não foi possível excluir o voluntário.";
        }
        return RedirectToAction("Index");
    }

    // ── helpers ────────────────────────────────────────────────

    private static VoluntarioModel Converter(Voluntario v) => new()
    {
        Codigo   = v.Codigo,
        Nome     = v.Nome,
        Telefone = v.Telefone,
        Funcao   = v.Funcao
    };

    private static Voluntario ConverterVoluntario(VoluntarioModel model) => new()
    {
        Codigo   = model.Codigo,
        Nome     = model.Nome,
        Telefone = string.IsNullOrWhiteSpace(model.Telefone) ? null : model.Telefone.Trim(),
        Funcao   = string.IsNullOrWhiteSpace(model.Funcao) ? null : model.Funcao.Trim()
    };
}
