using EM.Domain;
using EM.Domain.Fachadas;
using EM.Domain.Utilitarios;
using EM.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers;

public class AlunoController : Controller
{
    private readonly FachadaAluno _fachadaAluno;
    private readonly FachadaEncontro _fachadaEncontro;
    private readonly ILogger<AlunoController> _logger;

    public AlunoController(
        FachadaAluno fachadaAluno,
        FachadaEncontro fachadaEncontro,
        ILogger<AlunoController> logger)
    {
        _fachadaAluno = fachadaAluno;
        _fachadaEncontro = fachadaEncontro;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var lista = _fachadaAluno.ObterTodosAlunos().ConvertAll(ConverteAluno);
        return View(lista);
    }

    [HttpGet]
    public IActionResult BuscarPorMatricula(int matricula)
    {
        var aluno = _fachadaAluno.ObtenhaPorMatricula(matricula);
        var lista = aluno != null ? new[] { aluno } : Array.Empty<Aluno>();
        return Json(lista.Select(MapearJson));
    }

    [HttpGet]
    public IActionResult BuscarPorNome(string nome = "")
    {
        var lista = _fachadaAluno.ObtenhaPorNome(nome);
        return Json(lista.Select(MapearJson));
    }

    [HttpGet]
    public IActionResult Perfil(int id)
    {
        var aluno = _fachadaAluno.ObtenhaPorMatricula(id);
        if (aluno == null) return NotFound();

        var model = new PerfilAlunoModel
        {
            Aluno       = ConverteAluno(aluno),
            Turmas      = _fachadaAluno.ObterTurmasDoAluno(id).ConvertAll(t => new TurmaModel
            {
                Codigo = t.Codigo,
                Nome   = t.Nome,
                Ano    = t.Ano
            }),
            Presencas   = _fachadaEncontro.ObterPresencasDoAluno(id),
            Observacoes = _fachadaAluno.ObterObservacoes(id),
            Historico   = _fachadaAluno.ObterHistorico(id)
        };
        return View(model);
    }

    public IActionResult Edite()
    {
        return View(new AlunoModel());
    }

    public IActionResult Editar(int id)
    {
        var aluno = _fachadaAluno.ObtenhaPorMatricula(id);
        if (aluno == null) return NotFound();
        return View("Edite", ConverteAluno(aluno));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gravar(AlunoModel model, int sexo)
    {
        try
        {
            model.Sexo = EnumeradorSexo.ObtenhaPorCodigo(sexo);
            var matricula = _fachadaAluno.AdicionarAluno(ConvertaAluno(model));
            TempData["Sucesso"] = $"Aluno \"{model.Nome}\" cadastrado.";
            return RedirectToAction("Perfil", new { id = matricula });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edite", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Atualize(AlunoModel model, int sexo, int status)
    {
        try
        {
            model.Sexo = EnumeradorSexo.ObtenhaPorCodigo(sexo);
            model.Status = EnumeradorStatusAluno.ObtenhaPorCodigo(status);
            _fachadaAluno.AtualizeAluno(ConvertaAluno(model));

            TempData["Sucesso"] = $"Cadastro de \"{model.Nome}\" atualizado.";
            return RedirectToAction("Perfil", new { id = model.Matricula });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Edite", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AlterarStatus(int matricula, int status)
    {
        try
        {
            _fachadaAluno.AlterarStatus(matricula, status);
            TempData["Sucesso"] = "Status atualizado.";
        }
        catch (ArgumentException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Perfil", new { id = matricula });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AdicionarObservacao(int matricula, string texto)
    {
        try
        {
            _fachadaAluno.AdicionarObservacao(matricula, texto);
        }
        catch (ArgumentException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Perfil", new { id = matricula });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirObservacao(int codigo, int matricula)
    {
        _fachadaAluno.ExcluirObservacao(codigo);
        return RedirectToAction("Perfil", new { id = matricula });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "SomenteAdministrador")]
    public IActionResult Excluir(int id)
    {
        try
        {
            _fachadaAluno.ExcluirAluno(id);
            TempData["Sucesso"] = "Cadastro excluído definitivamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir o aluno {Matricula}.", id);
            TempData["Erro"] = "Não foi possível excluir o cadastro.";
        }
        return RedirectToAction("Index");
    }

    // ── helpers ────────────────────────────────────────────────

    private static object MapearJson(Aluno a) => new
    {
        matricula    = a.Matricula,
        nome         = a.Nome,
        idade        = a.Idade,
        sexo         = a.Sexo.Descricao,
        nascimento   = a.Nascimento.ToString("dd/MM/yyyy"),
        cidade       = MontarLocal(a),
        status       = a.Status.Descricao,
        statusCodigo = a.Status.Codigo
    };

    private static string MontarLocal(Aluno a)
    {
        var partes = new[] { a.Bairro, a.Cidade }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", partes);
    }

    private static AlunoModel ConverteAluno(Aluno aluno) => new()
    {
        Matricula             = aluno.Matricula,
        Nome                  = aluno.Nome,
        Sexo                  = aluno.Sexo,
        DataNascimento        = aluno.Nascimento,
        Cidade                = aluno.Cidade,
        Bairro                = aluno.Bairro,
        ResponsavelNome       = aluno.ResponsavelNome,
        ResponsavelTelefone   = aluno.ResponsavelTelefone,
        ResponsavelParentesco = aluno.ResponsavelParentesco,
        Status                = aluno.Status
    };

    private static Aluno ConvertaAluno(AlunoModel model) => new()
    {
        Matricula             = model.Matricula,
        Nome                  = model.Nome,
        Sexo                  = model.Sexo,
        Nascimento            = model.DataNascimento,
        Cidade                = model.Cidade,
        Bairro                = model.Bairro,
        ResponsavelNome       = model.ResponsavelNome,
        ResponsavelTelefone   = model.ResponsavelTelefone,
        ResponsavelParentesco = model.ResponsavelParentesco,
        Status                = model.Status
    };
}
