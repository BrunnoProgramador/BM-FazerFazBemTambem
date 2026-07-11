using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EM.Web.Infraestrutura;

/// <summary>
/// Enquanto o usuário tiver senha provisória (claim "trocar_senha"),
/// qualquer navegação é redirecionada para a tela de troca de senha.
/// Só a própria troca e o sair ficam liberados.
/// </summary>
public class FiltroTrocaSenhaObrigatoria : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext contexto)
    {
        var usuario = contexto.HttpContext.User;
        if (usuario.Identity?.IsAuthenticated != true) return;
        if (!usuario.HasClaim("trocar_senha", "1")) return;

        var controller = contexto.RouteData.Values["controller"] as string ?? "";
        var action = contexto.RouteData.Values["action"] as string ?? "";

        var liberado = controller.Equals("Conta", StringComparison.OrdinalIgnoreCase)
            && (action.Equals("TrocarSenha", StringComparison.OrdinalIgnoreCase)
                || action.Equals("Sair", StringComparison.OrdinalIgnoreCase));

        if (!liberado)
            contexto.Result = new RedirectToActionResult("TrocarSenha", "Conta", null);
    }

    public void OnActionExecuted(ActionExecutedContext contexto) { }
}
