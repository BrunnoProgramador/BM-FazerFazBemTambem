using System.Security.Claims;
using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

/// <summary>Configurações → usuários do sistema. Acesso exclusivo do administrador.</summary>
[Authorize(Policy = "SomenteAdministrador")]
public class UsuarioController : Controller
{
    private readonly FachadaUsuario _fachada;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(FachadaUsuario fachada, ILogger<UsuarioController> logger)
    {
        _fachada = fachada;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.Usuarios = _fachada.ObterTodos();
        return View(new NovoUsuarioModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cadastrar(NovoUsuarioModel model)
    {
        try
        {
            var usuario = _fachada.CadastrarUsuario(
                model.Login, model.Nome, model.SenhaProvisoria, model.Confirmacao);

            _logger.LogInformation("Usuário {Login} cadastrado pelo administrador.", usuario.Login);
            TempData["Sucesso"] = $"Usuário \"{usuario.Login}\" criado. " +
                "Informe a senha provisória à pessoa — ela será obrigada a trocá-la no primeiro acesso.";
            return RedirectToAction("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Usuarios = _fachada.ObterTodos();
            return View("Index", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id)
    {
        try
        {
            var solicitante = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _fachada.ExcluirUsuario(id, solicitante);
            TempData["Sucesso"] = "Usuário excluído.";
        }
        catch (ArgumentException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir o usuário {Codigo}.", id);
            TempData["Erro"] = "Não foi possível excluir o usuário.";
        }
        return RedirectToAction("Index");
    }
}
