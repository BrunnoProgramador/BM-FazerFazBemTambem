using EM.Domain;
using EM.Domain.Fachadas;
using EM.Domain.Interfaces;
using EM.Domain.Utilitarios;
using Xunit;

namespace EM.Testes.Fachadas;

public class FachadaAlunoTestes
{
    private readonly FakeRepositorioAluno _repositorio = new();
    private readonly FachadaAluno _fachada;

    public FachadaAlunoTestes()
    {
        _fachada = new FachadaAluno(_repositorio);
    }

    private static Aluno AlunoValido() => new()
    {
        Nome = "João Pedro",
        Nascimento = DateTime.Today.AddYears(-9),
        Sexo = EnumeradorSexo.Masculino
    };

    [Fact]
    public void Adicionar_registra_historico_de_cadastro()
    {
        var matricula = _fachada.AdicionarAluno(AlunoValido());

        Assert.True(matricula > 0);
        var historico = _fachada.ObterHistorico(matricula);
        Assert.Single(historico);
        Assert.Contains("Cadastrado", historico[0].Texto);
    }

    [Fact]
    public void Adicionar_aluno_invalido_lanca_excecao_e_nao_grava()
    {
        var aluno = AlunoValido();
        aluno.Nome = "";

        Assert.Throws<ArgumentException>(() => _fachada.AdicionarAluno(aluno));
        Assert.Empty(_fachada.ObterTodosAlunos());
    }

    [Fact]
    public void Mudanca_de_status_na_atualizacao_registra_historico()
    {
        var matricula = _fachada.AdicionarAluno(AlunoValido());

        var aluno = _fachada.ObtenhaPorMatricula(matricula)!;
        aluno.Status = EnumeradorStatusAluno.ParouDeFrequentar;
        _fachada.AtualizeAluno(aluno);

        var historico = _fachada.ObterHistorico(matricula);
        Assert.Equal(2, historico.Count); // cadastro + mudança de status
        Assert.Contains(historico, h => h.Texto.Contains("Parou de frequentar"));
    }

    [Fact]
    public void Atualizacao_sem_mudanca_de_status_nao_gera_historico_extra()
    {
        var matricula = _fachada.AdicionarAluno(AlunoValido());

        var aluno = _fachada.ObtenhaPorMatricula(matricula)!;
        aluno.Nome = "João Pedro da Silva";
        _fachada.AtualizeAluno(aluno);

        Assert.Single(_fachada.ObterHistorico(matricula)); // só o cadastro
    }

    [Fact]
    public void AlterarStatus_para_o_mesmo_status_nao_faz_nada()
    {
        var matricula = _fachada.AdicionarAluno(AlunoValido());

        _fachada.AlterarStatus(matricula, EnumeradorStatusAluno.Ativo.Codigo);

        Assert.Single(_fachada.ObterHistorico(matricula));
    }

    [Fact]
    public void Observacao_vazia_lanca_excecao()
    {
        var matricula = _fachada.AdicionarAluno(AlunoValido());
        Assert.Throws<ArgumentException>(() => _fachada.AdicionarObservacao(matricula, "   "));
    }

    [Fact]
    public void Observacao_valida_eh_gravada_com_texto_aparado()
    {
        var matricula = _fachada.AdicionarAluno(AlunoValido());
        _fachada.AdicionarObservacao(matricula, "  Possui alergia a amendoim  ");

        var observacoes = _fachada.ObterObservacoes(matricula);
        Assert.Single(observacoes);
        Assert.Equal("Possui alergia a amendoim", observacoes[0].Texto);
    }
}

/// <summary>Repositório em memória para testes da fachada.</summary>
internal class FakeRepositorioAluno : IRepositorioAluno
{
    private readonly List<Aluno> _alunos = new();
    private readonly List<ObservacaoAluno> _observacoes = new();
    private readonly List<HistoricoAluno> _historico = new();
    private int _proximaMatricula = 1;
    private int _proximoCodigo = 1;

    // Leituras devolvem cópias, como um banco de verdade faria —
    // sem isso a comparação de status em AtualizeAluno seria mascarada.
    public Aluno? GetByMatricula(int matricula)
    {
        var aluno = _alunos.FirstOrDefault(a => a.Matricula == matricula);
        return aluno == null ? null : Clonar(aluno);
    }

    public IEnumerable<Aluno> GetByContendoNoNome(string parteDoNome) =>
        _alunos.Where(a => a.Nome.Contains(parteDoNome, StringComparison.OrdinalIgnoreCase))
               .Select(Clonar);

    public int Adicionar(Aluno entidade)
    {
        entidade.Matricula = _proximaMatricula++;
        _alunos.Add(Clonar(entidade));
        return entidade.Matricula;
    }

    public void Atualizar(Aluno entidade)
    {
        var indice = _alunos.FindIndex(a => a.Matricula == entidade.Matricula);
        if (indice >= 0) _alunos[indice] = Clonar(entidade);
    }

    public void Excluir(int matricula) =>
        _alunos.RemoveAll(a => a.Matricula == matricula);

    public IEnumerable<Aluno> ObterTodos() => _alunos.Select(Clonar);

    private static Aluno Clonar(Aluno a) => new()
    {
        Matricula             = a.Matricula,
        Nome                  = a.Nome,
        Nascimento            = a.Nascimento,
        Sexo                  = a.Sexo,
        Cidade                = a.Cidade,
        Bairro                = a.Bairro,
        ResponsavelNome       = a.ResponsavelNome,
        ResponsavelTelefone   = a.ResponsavelTelefone,
        ResponsavelParentesco = a.ResponsavelParentesco,
        Status                = a.Status
    };

    public void AtualizarStatus(int matricula, int statusCodigo)
    {
        var aluno = _alunos.FirstOrDefault(a => a.Matricula == matricula);
        if (aluno != null)
            aluno.Status = EnumeradorStatusAluno.ObtenhaPorCodigo(statusCodigo);
    }

    public IEnumerable<Turma> ObterTurmasDoAluno(int matricula) => Enumerable.Empty<Turma>();

    public IEnumerable<ObservacaoAluno> ObterObservacoes(int matricula) =>
        _observacoes.Where(o => o.AlunoMatricula == matricula);

    public void AdicionarObservacao(ObservacaoAluno observacao)
    {
        observacao.Codigo = _proximoCodigo++;
        _observacoes.Add(observacao);
    }

    public void ExcluirObservacao(int codigo) =>
        _observacoes.RemoveAll(o => o.Codigo == codigo);

    public IEnumerable<HistoricoAluno> ObterHistorico(int matricula) =>
        _historico.Where(h => h.AlunoMatricula == matricula);

    public void RegistrarHistorico(int matricula, string texto)
    {
        _historico.Add(new HistoricoAluno
        {
            Codigo = _proximoCodigo++,
            AlunoMatricula = matricula,
            Data = DateTime.Today,
            Texto = texto
        });
    }
}
