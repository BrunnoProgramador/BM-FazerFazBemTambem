using System.Security.Claims;
using EM.Domain;
using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class ContaController : Controller
{
    private readonly FachadaUsuario _fachada;
    private readonly ILogger<ContaController> _logger;

    public ContaController(FachadaUsuario fachada, ILogger<ContaController> logger)
    {
        _fachada = fachada;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!_fachada.ExisteUsuario())
            return RedirectToAction("Configurar");

        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
    {
        var usuario = _fachada.Autenticar(model.Login, model.Senha);

        if (usuario == null)
        {
            _logger.LogWarning("Tentativa de login inválida para o usuário {Login}.", model.Login);
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        await EntrarAsync(usuario, model.Lembrar);
        _logger.LogInformation("Usuário {Login} entrou no sistema.", usuario.Login);

        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Dashboard");
    }

    /// <summary>Configuração inicial: cria o primeiro usuário quando o sistema está vazio.</summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Configurar()
    {
        if (_fachada.ExisteUsuario())
            return RedirectToAction("Login");

        return View(new PrimeiroUsuarioModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Configurar(PrimeiroUsuarioModel model)
    {
        if (_fachada.ExisteUsuario())
            return RedirectToAction("Login");

        try
        {
            var usuario = _fachada.CriarPrimeiroUsuario(
                model.Login, model.Nome, model.Senha, model.ConfirmacaoSenha);

            await EntrarAsync(usuario, lembrar: true);
            _logger.LogInformation("Primeiro usuário {Login} criado e autenticado.", usuario.Login);

            TempData["Sucesso"] = $"Bem-vindo(a), {usuario.Nome}! O sistema está pronto para uso.";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        var login = User.Identity?.Name;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Usuário {Login} saiu do sistema.", login);
        return RedirectToAction("Login");
    }

    // ── helpers ────────────────────────────────────────────────

    private async Task EntrarAsync(Usuario usuario, bool lembrar)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Codigo.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new("login", usuario.Login)
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            new AuthenticationProperties
            {
                IsPersistent = lembrar,
                ExpiresUtc = lembrar ? DateTimeOffset.UtcNow.AddDays(30) : null
            });
    }
}
