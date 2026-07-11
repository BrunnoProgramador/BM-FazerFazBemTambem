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

        // Senha provisória: obriga a troca antes de qualquer outra tela
        if (usuario.DeveTrocarSenha)
            return RedirectToAction("TrocarSenha");

        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Dashboard");
    }

    /// <summary>
    /// Primeiro acesso: o usuário é "administrador" (ou "admin", tanto faz) —
    /// a pessoa só define a senha dele aqui.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Configurar()
    {
        if (_fachada.ExisteUsuario())
            return RedirectToAction("Login");

        return View(new SenhaAdministradorModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Configurar(SenhaAdministradorModel model)
    {
        if (_fachada.ExisteUsuario())
            return RedirectToAction("Login");

        try
        {
            var usuario = _fachada.CriarAdministrador(model.Senha, model.Confirmacao);

            await EntrarAsync(usuario, lembrar: true);
            _logger.LogInformation("Administrador criado e autenticado no primeiro acesso.");

            TempData["Sucesso"] = "Senha do administrador definida. O sistema está pronto para uso!";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // ── troca de senha (obrigatória quando provisória) ─────────

    [HttpGet]
    public IActionResult TrocarSenha()
    {
        return View(new TrocarSenhaModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrocarSenha(TrocarSenhaModel model)
    {
        try
        {
            var codigo = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = _fachada.TrocarSenha(codigo, model.NovaSenha, model.Confirmacao);

            // Reemite o cookie sem a marcação de troca pendente
            await EntrarAsync(usuario, lembrar: false);
            _logger.LogInformation("Usuário {Login} trocou a própria senha.", usuario.Login);

            TempData["Sucesso"] = "Senha alterada com sucesso.";
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

        if (usuario.EhAdministrador)
            claims.Add(new Claim("admin", "1"));

        if (usuario.DeveTrocarSenha)
            claims.Add(new Claim("trocar_senha", "1"));

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
