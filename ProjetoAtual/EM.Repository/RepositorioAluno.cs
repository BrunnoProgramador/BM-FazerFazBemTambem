using EM.Domain;
using EM.Domain.Interfaces;
using EM.Domain.Utilitarios;
using Npgsql;

namespace EM.Repository;

public class RepositorioAluno : RepositorioAbstrato<Aluno>, IRepositorioAluno
{
    private const string ColunasAluno =
        "ALUNMATRICULA, ALUNNOME, ALUNCPF, ALUNDTNASC, ALUNSEXO, " +
        "ALUNCIDADETXT, ALUNBAIRRO, ALUNRESPNOME, ALUNRESPTEL, ALUNRESPPAR, ALUNSTATUS";

    public RepositorioAluno(string connectionString) : base(connectionString) { }

    public int Adicionar(Aluno entidade)
    {
        var matricula = ExecutarEscalar(
            "INSERT INTO TBALUNO (ALUNMATRICULA, ALUNNOME, ALUNCPF, ALUNDTNASC, ALUNSEXO, " +
            "ALUNCIDADETXT, ALUNBAIRRO, ALUNRESPNOME, ALUNRESPTEL, ALUNRESPPAR, ALUNSTATUS) " +
            "VALUES (nextval('gen_tbaluno'), @nome, @cpf, @dtnasc, @sexo, " +
            "@cidade, @bairro, @respnome, @resptel, @resppar, @status) " +
            "RETURNING ALUNMATRICULA",
            p => PreencherParametros(p, entidade));

        return Convert.ToInt32(matricula);
    }

    public override void Atualizar(Aluno entidade)
    {
        ExecutarComando(
            "UPDATE TBALUNO SET ALUNNOME = @nome, ALUNCPF = @cpf, ALUNDTNASC = @dtnasc, " +
            "ALUNSEXO = @sexo, ALUNCIDADETXT = @cidade, ALUNBAIRRO = @bairro, " +
            "ALUNRESPNOME = @respnome, ALUNRESPTEL = @resptel, ALUNRESPPAR = @resppar, " +
            "ALUNSTATUS = @status WHERE ALUNMATRICULA = @matricula",
            p =>
            {
                PreencherParametros(p, entidade);
                p.Add(new NpgsqlParameter("@matricula", entidade.Matricula));
            });
    }

    public override void Excluir(int matricula)
    {
        // Remove o aluno e todos os vínculos em uma única transação.
        using var conexao = CriarConexao();
        using var transacao = conexao.BeginTransaction();

        var comandos = new List<string>
        {
            "DELETE FROM TBOBSALUNO    WHERE OBSALUNO      = @m",
            "DELETE FROM TBHISTALUNO   WHERE HISALUNO      = @m",
            "DELETE FROM TBENCPRESENCA WHERE ALUNMATRICULA = @m",
            "DELETE FROM TBEVENTOALUNO WHERE ALUNMATRICULA = @m",
            "DELETE FROM TBTURMA_ALUNO WHERE ALUNMATRICULA = @m"
        };

        // Tabela de frequência legada, se existir neste banco
        if (TabelaExiste(conexao, transacao, "TBFREQUENCIA"))
            comandos.Add("DELETE FROM TBFREQUENCIA WHERE FREQALUNO = @m");

        comandos.Add("DELETE FROM TBALUNO WHERE ALUNMATRICULA = @m");

        foreach (var sql in comandos)
        {
            using var comando = new NpgsqlCommand(sql, conexao, transacao);
            comando.Parameters.Add(new NpgsqlParameter("@m", matricula));
            comando.ExecuteNonQuery();
        }

        transacao.Commit();
    }

    public override IEnumerable<Aluno> ObterTodos()
    {
        return Consultar(
            $"SELECT {ColunasAluno} FROM TBALUNO ORDER BY ALUNNOME",
            MapearAluno);
    }

    public Aluno? GetByMatricula(int matricula)
    {
        return Consultar(
            $"SELECT {ColunasAluno} FROM TBALUNO WHERE ALUNMATRICULA = @matricula",
            MapearAluno,
            p => p.Add(new NpgsqlParameter("@matricula", matricula))).FirstOrDefault();
    }

    public IEnumerable<Aluno> GetByContendoNoNome(string parteDoNome)
    {
        return Consultar(
            $"SELECT {ColunasAluno} FROM TBALUNO " +
            "WHERE UPPER(ALUNNOME) LIKE UPPER(@nome) ORDER BY ALUNNOME",
            MapearAluno,
            p => p.Add(new NpgsqlParameter("@nome", $"%{parteDoNome?.Trim()}%")));
    }

    public void AtualizarStatus(int matricula, int statusCodigo)
    {
        ExecutarComando(
            "UPDATE TBALUNO SET ALUNSTATUS = @status WHERE ALUNMATRICULA = @matricula",
            p =>
            {
                p.Add(new NpgsqlParameter("@status", statusCodigo));
                p.Add(new NpgsqlParameter("@matricula", matricula));
            });
    }

    public IEnumerable<Turma> ObterTurmasDoAluno(int matricula)
    {
        return ConsultarComo(
            @"SELECT t.TURMCODIGO, t.TURMNOME, t.TURMAANO, t.TURMPROFESSOR, t.TURMDIAS, t.TURMHORARIO
                FROM TBTURMA t
                INNER JOIN TBTURMA_ALUNO ta ON ta.TURMCODIGO = t.TURMCODIGO
               WHERE ta.ALUNMATRICULA = @matricula
               ORDER BY t.TURMAANO DESC, t.TURMNOME",
            reader => new Turma
            {
                Codigo     = reader.GetInt32(0),
                Nome       = reader.GetString(1),
                Ano        = reader.GetInt32(2),
                Professor  = reader.IsDBNull(3) ? null : reader.GetString(3),
                DiasSemana = reader.IsDBNull(4) ? null : reader.GetString(4),
                Horario    = reader.IsDBNull(5) ? null : reader.GetString(5)
            },
            p => p.Add(new NpgsqlParameter("@matricula", matricula)));
    }

    // ── observações ────────────────────────────────────────────

    public IEnumerable<ObservacaoAluno> ObterObservacoes(int matricula)
    {
        return ConsultarComo(
            "SELECT OBSCODIGO, OBSALUNO, OBSDATA, OBSTEXTO FROM TBOBSALUNO " +
            "WHERE OBSALUNO = @matricula ORDER BY OBSCODIGO DESC",
            reader => new ObservacaoAluno
            {
                Codigo         = reader.GetInt32(0),
                AlunoMatricula = reader.GetInt32(1),
                Data           = ConverterData(reader.GetInt32(2)),
                Texto          = reader.GetString(3)
            },
            p => p.Add(new NpgsqlParameter("@matricula", matricula)));
    }

    public void AdicionarObservacao(ObservacaoAluno observacao)
    {
        ExecutarComando(
            "INSERT INTO TBOBSALUNO (OBSCODIGO, OBSALUNO, OBSDATA, OBSTEXTO) " +
            "VALUES (nextval('gen_tbobsaluno'), @aluno, @data, @texto)",
            p =>
            {
                p.Add(new NpgsqlParameter("@aluno", observacao.AlunoMatricula));
                p.Add(new NpgsqlParameter("@data",  int.Parse(observacao.Data.ToString("yyyyMMdd"))));
                p.Add(new NpgsqlParameter("@texto", observacao.Texto));
            });
    }

    public void ExcluirObservacao(int codigo)
    {
        ExecutarComando(
            "DELETE FROM TBOBSALUNO WHERE OBSCODIGO = @codigo",
            p => p.Add(new NpgsqlParameter("@codigo", codigo)));
    }

    // ── histórico ──────────────────────────────────────────────

    public IEnumerable<HistoricoAluno> ObterHistorico(int matricula)
    {
        return ConsultarComo(
            "SELECT HISCODIGO, HISALUNO, HISDATA, HISTEXTO FROM TBHISTALUNO " +
            "WHERE HISALUNO = @matricula ORDER BY HISCODIGO DESC",
            reader => new HistoricoAluno
            {
                Codigo         = reader.GetInt32(0),
                AlunoMatricula = reader.GetInt32(1),
                Data           = ConverterData(reader.GetInt32(2)),
                Texto          = reader.GetString(3)
            },
            p => p.Add(new NpgsqlParameter("@matricula", matricula)));
    }

    public void RegistrarHistorico(int matricula, string texto)
    {
        ExecutarComando(
            "INSERT INTO TBHISTALUNO (HISCODIGO, HISALUNO, HISDATA, HISTEXTO) " +
            "VALUES (nextval('gen_tbhistaluno'), @aluno, @data, @texto)",
            p =>
            {
                p.Add(new NpgsqlParameter("@aluno", matricula));
                p.Add(new NpgsqlParameter("@data",  int.Parse(DateTime.Today.ToString("yyyyMMdd"))));
                p.Add(new NpgsqlParameter("@texto", texto));
            });
    }

    // ── helpers ────────────────────────────────────────────────

    private static void PreencherParametros(NpgsqlParameterCollection p, Aluno entidade)
    {
        p.Add(new NpgsqlParameter("@nome",     entidade.Nome));
        p.Add(new NpgsqlParameter("@cpf",      (object?)entidade.CPF ?? DBNull.Value));
        p.Add(new NpgsqlParameter("@dtnasc",   int.Parse(entidade.Nascimento.ToString("yyyyMMdd"))));
        p.Add(new NpgsqlParameter("@sexo",     entidade.Sexo.Codigo));
        p.Add(new NpgsqlParameter("@cidade",   ValorOuNulo(entidade.Cidade)));
        p.Add(new NpgsqlParameter("@bairro",   ValorOuNulo(entidade.Bairro)));
        p.Add(new NpgsqlParameter("@respnome", ValorOuNulo(entidade.ResponsavelNome)));
        p.Add(new NpgsqlParameter("@resptel",  ValorOuNulo(entidade.ResponsavelTelefone)));
        p.Add(new NpgsqlParameter("@resppar",  ValorOuNulo(entidade.ResponsavelParentesco)));
        p.Add(new NpgsqlParameter("@status",   entidade.Status.Codigo));
    }

    private static object ValorOuNulo(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();

    private static Aluno MapearAluno(NpgsqlDataReader reader) => new()
    {
        Matricula             = reader.GetInt32(0),
        Nome                  = reader.GetString(1),
        CPF                   = reader.IsDBNull(2) ? null : reader.GetString(2),
        Nascimento            = ConverterData(reader.GetInt32(3)),
        Sexo                  = EnumeradorSexo.ObtenhaPorCodigo(reader.GetInt32(4)),
        Cidade                = reader.IsDBNull(5) ? null : reader.GetString(5),
        Bairro                = reader.IsDBNull(6) ? null : reader.GetString(6),
        ResponsavelNome       = reader.IsDBNull(7) ? null : reader.GetString(7),
        ResponsavelTelefone   = reader.IsDBNull(8) ? null : reader.GetString(8),
        ResponsavelParentesco = reader.IsDBNull(9) ? null : reader.GetString(9),
        Status                = EnumeradorStatusAluno.ObtenhaPorCodigo(reader.GetInt32(10))
    };

    private static DateTime ConverterData(int dataYyyyMmDd) =>
        DateTime.ParseExact(dataYyyyMmDd.ToString(), "yyyyMMdd", null);

    private static bool TabelaExiste(NpgsqlConnection conexao, NpgsqlTransaction transacao, string nome)
    {
        using var comando = new NpgsqlCommand(
            "SELECT COUNT(*) FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = @nome",
            conexao, transacao);
        comando.Parameters.Add(new NpgsqlParameter("@nome", nome));
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }
}
