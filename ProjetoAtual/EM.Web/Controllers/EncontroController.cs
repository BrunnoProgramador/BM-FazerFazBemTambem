using EM.Domain;
using EM.Domain.Fachadas;
using EM.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EM.Web.Controllers;

/// <summary>Frequência por encontro: uma turma, uma data, uma lista de presença.</summary>
public class EncontroController : Controller
{
    private readonly FachadaEncontro _fachadaEncontro;
    private readonly FachadaTurma _fachadaTurma;
    private readonly ILogger<EncontroController> _logger;

    public EncontroController(
        FachadaEncontro fachadaEncontro,
        FachadaTurma fachadaTurma,
        ILogger<EncontroController> logger)
    {
        _fachadaEncontro = fachadaEncontro;
        _fachadaTurma = fachadaTurma;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var lista = _fachadaEncontro.ObterTodos().ConvertAll(Converter);
        return View(lista);
    }

    [HttpGet]
    public IActionResult Registrar(int? turma)
    {
        CarregarTurmas(turma);

        var model = new EncontroModel { Data = DateTime.Today };

        if (turma.HasValue && turma.Value > 0)
        {
            var codigo = turma.Value;
            var turmaSelecionada = _fachadaTurma.ObterPorCodigo(codigo);
            if (turmaSelecionada == null) return NotFound();

            model.TurmaCodigo = codigo;
            model.TurmaNome = turmaSelecionada.Nome;
            model.Presencas = _fachadaTurma.ObterAlunosDaTurma(codigo)
                .ConvertAll(a => new PresencaModel
                {
                    Matricula = a.Matricula,
                    Nome = a.Nome,
                    Presente = true // padrão: todos presentes, desmarca quem faltou
                });
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var encontro = _fachadaEncontro.ObterPorCodigo(id);
        if (encontro == null) return NotFound();

        CarregarTurmas(encontro.Turma.Codigo);
        return View("Registrar", Converter(encontro));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gravar(int turmaCodigo, DateTime data, int[] matriculas, int[] presentes)
    {
        var encontro = MontarEncontro(0, turmaCodigo, data, matriculas, presentes);

        try
        {
            _fachadaEncontro.Registrar(encontro);
            TempData["Sucesso"] = $"Frequência de {data:dd/MM/yyyy} registrada " +
                $"({encontro.Presencas.Count(p => p.Presente)} de {encontro.Presencas.Count} presentes).";
            return RedirectToAction("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            CarregarTurmas(turmaCodigo);
            return View("Registrar", ConverterFormulario(encontro));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Atualize(int codigo, int turmaCodigo, DateTime data, int[] matriculas, int[] presentes)
    {
        var encontro = MontarEncontro(codigo, turmaCodigo, data, matriculas, presentes);

        try
        {
            _fachadaEncontro.Atualizar(encontro);
            TempData["Sucesso"] = "Registro de frequência atualizado.";
            return RedirectToAction("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            CarregarTurmas(turmaCodigo);
            return View("Registrar", ConverterFormulario(encontro));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id)
    {
        try
        {
            _fachadaEncontro.Excluir(id);
            TempData["Sucesso"] = "Registro de frequência excluído.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir o encontro {Codigo}.", id);
            TempData["Erro"] = "Não foi possível excluir o registro.";
        }
        return RedirectToAction("Index");
    }

    // ── helpers ────────────────────────────────────────────────

    private void CarregarTurmas(int? selecionada = null)
    {
        var turmas = _fachadaTurma.ObterTodas();
        ViewBag.Turmas = new SelectList(turmas, "Codigo", "Nome", selecionada);
    }

    private Encontro MontarEncontro(int codigo, int turmaCodigo, DateTime data, int[] matriculas, int[] presentes)
    {
        var setPresentes = presentes.ToHashSet();
        var nomes = turmaCodigo > 0
            ? _fachadaTurma.ObterAlunosDaTurma(turmaCodigo).ToDictionary(a => a.Matricula, a => a.Nome)
            : new Dictionary<int, string>();

        // Só aceita matrículas de alunos da turma. Na edição, alunos que
        // já constavam no encontro continuam válidos mesmo que tenham
        // saído da turma depois — presença histórica não se perde.
        var permitidos = new HashSet<int>(nomes.Keys);
        if (codigo > 0)
        {
            var existente = _fachadaEncontro.ObterPorCodigo(codigo);
            if (existente != null)
            {
                foreach (var presenca in existente.Presencas)
                {
                    permitidos.Add(presenca.Aluno.Matricula);
                    nomes.TryAdd(presenca.Aluno.Matricula, presenca.Aluno.Nome);
                }
            }
        }

        return new Encontro
        {
            Codigo = codigo,
            Turma = new Turma { Codigo = turmaCodigo, Nome = _fachadaTurma.ObterPorCodigo(turmaCodigo)?.Nome },
            Data = data,
            Presencas = matriculas
                .Distinct()
                .Where(permitidos.Contains)
                .Select(m => new PresencaAluno
                {
                    Aluno = new Aluno { Matricula = m, Nome = nomes.GetValueOrDefault(m, m.ToString()) },
                    Presente = setPresentes.Contains(m)
                }).ToList()
        };
    }

    private static EncontroModel Converter(Encontro e) => new()
    {
        Codigo         = e.Codigo,
        TurmaCodigo    = e.Turma.Codigo,
        TurmaNome      = e.Turma.Nome,
        Data           = e.Data,
        TotalAlunos    = e.TotalAlunos,
        TotalPresentes = e.TotalPresentes,
        Presencas      = e.Presencas.ConvertAll(p => new PresencaModel
        {
            Matricula = p.Aluno.Matricula,
            Nome      = p.Aluno.Nome,
            Presente  = p.Presente
        })
    };

    /// <summary>Reconstrói o model do formulário após falha de validação.</summary>
    private static EncontroModel ConverterFormulario(Encontro e) => new()
    {
        Codigo      = e.Codigo,
        TurmaCodigo = e.Turma?.Codigo ?? 0,
        TurmaNome   = e.Turma?.Nome,
        Data        = e.Data,
        Presencas   = e.Presencas.ConvertAll(p => new PresencaModel
        {
            Matricula = p.Aluno.Matricula,
            Nome      = p.Aluno.Nome,
            Presente  = p.Presente
        })
    };
}
