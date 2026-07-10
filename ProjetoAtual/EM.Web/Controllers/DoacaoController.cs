using EM.Domain;
using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class DoacaoController : Controller
{
    private readonly FachadaDoacao _fachada;
    private readonly ILogger<DoacaoController> _logger;

    public DoacaoController(FachadaDoacao fachada, ILogger<DoacaoController> logger)
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
        return View(new DoacaoModel { Data = DateTime.Today });
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var doacao = _fachada.ObterPorCodigo(id);
        if (doacao == null) return NotFound();
        return View("Edite", Converter(doacao));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gravar(DoacaoModel model)
    {
        try
        {
            _fachada.Adicionar(ConverterDoacao(model));
            TempData["Sucesso"] = "Doação registrada.";
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
    public IActionResult Atualize(DoacaoModel model)
    {
        try
        {
            _fachada.Atualizar(ConverterDoacao(model));
            TempData["Sucesso"] = "Doação atualizada.";
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
            TempData["Sucesso"] = "Doação excluída.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir a doação {Codigo}.", id);
            TempData["Erro"] = "Não foi possível excluir a doação.";
        }
        return RedirectToAction("Index");
    }

    // ── helpers ────────────────────────────────────────────────

    private static DoacaoModel Converter(Doacao d) => new()
    {
        Codigo     = d.Codigo,
        Item       = d.Item,
        Quantidade = d.Quantidade,
        Data       = d.Data,
        Doador     = d.Doador
    };

    private static Doacao ConverterDoacao(DoacaoModel model) => new()
    {
        Codigo     = model.Codigo,
        Item       = model.Item,
        Quantidade = model.Quantidade,
        Data       = model.Data,
        Doador     = string.IsNullOrWhiteSpace(model.Doador) ? null : model.Doador.Trim()
    };
}
