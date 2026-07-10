using EM.Domain;
using EM.Domain.Interfaces;
using Npgsql;

namespace EM.Repository;

public class RepositorioEvento : RepositorioAbstrato<Evento>, IRepositorioEvento
{
    public RepositorioEvento(string connectionString) : base(connectionString) { }

    public void Adicionar(Evento entidade)
    {
        ExecutarComando(
            "INSERT INTO TBEVENTO (EVECODIGO, EVENOME, EVEDATA, EVEOBS) " +
            "VALUES (nextval('gen_tbevento'), @nome, @data, @obs)",
            p =>
            {
                p.Add(new NpgsqlParameter("@nome", entidade.Nome));
                p.Add(new NpgsqlParameter("@data", ParaInteiro(entidade.Data)));
                p.Add(new NpgsqlParameter("@obs",  (object?)entidade.Observacao ?? DBNull.Value));
            });
    }

    public override void Atualizar(Evento entidade)
    {
        ExecutarComando(
            "UPDATE TBEVENTO SET EVENOME = @nome, EVEDATA = @data, EVEOBS = @obs " +
            "WHERE EVECODIGO = @codigo",
            p =>
            {
                p.Add(new NpgsqlParameter("@nome",   entidade.Nome));
                p.Add(new NpgsqlParameter("@data",   ParaInteiro(entidade.Data)));
                p.Add(new NpgsqlParameter("@obs",    (object?)entidade.Observacao ?? DBNull.Value));
                p.Add(new NpgsqlParameter("@codigo", entidade.Codigo));
            });
    }

    public override void Excluir(int codigo)
    {
        using var conexao = CriarConexao();
        using var transacao = conexao.BeginTransaction();

        foreach (var sql in new[]
        {
            "DELETE FROM TBEVENTOALUNO WHERE EVECODIGO = @codigo",
            "DELETE FROM TBEVENTO WHERE EVECODIGO = @codigo"
        })
        {
            using var comando = new NpgsqlCommand(sql, conexao, transacao);
            comando.Parameters.Add(new NpgsqlParameter("@codigo", codigo));
            comando.ExecuteNonQuery();
        }

        transacao.Commit();
    }

    public override IEnumerable<Evento> ObterTodos()
    {
        return Consultar(
            @"SELECT e.EVECODIGO, e.EVENOME, e.EVEDATA, e.EVEOBS,
                     COUNT(ea.ALUNMATRICULA) AS PARTICIPANTES
                FROM TBEVENTO e
                LEFT JOIN TBEVENTOALUNO ea ON ea.EVECODIGO = e.EVECODIGO
               GROUP BY e.EVECODIGO, e.EVENOME, e.EVEDATA, e.EVEOBS
               ORDER BY e.EVEDATA DESC, e.EVENOME",
            reader => new Evento
            {
                Codigo             = reader.GetInt32(0),
                Nome               = reader.GetString(1),
                Data               = ConverterData(reader.GetInt32(2)),
                Observacao         = reader.IsDBNull(3) ? null : reader.GetString(3),
                TotalParticipantes = reader.GetInt32(4)
            });
    }

    public Evento? GetByCodigo(int codigo)
    {
        return Consultar(
            @"SELECT e.EVECODIGO, e.EVENOME, e.EVEDATA, e.EVEOBS,
                     (SELECT COUNT(*) FROM TBEVENTOALUNO ea WHERE ea.EVECODIGO = e.EVECODIGO)
                FROM TBEVENTO e
               WHERE e.EVECODIGO = @codigo",
            reader => new Evento
            {
                Codigo             = reader.GetInt32(0),
                Nome               = reader.GetString(1),
                Data               = ConverterData(reader.GetInt32(2)),
                Observacao         = reader.IsDBNull(3) ? null : reader.GetString(3),
                TotalParticipantes = reader.GetInt32(4)
            },
            p => p.Add(new NpgsqlParameter("@codigo", codigo))).FirstOrDefault();
    }

    public void AdicionarParticipante(int eventoCodigo, int matricula)
    {
        ExecutarComando(
            "INSERT INTO TBEVENTOALUNO (EVECODIGO, ALUNMATRICULA) VALUES (@evento, @aluno)",
            p =>
            {
                p.Add(new NpgsqlParameter("@evento", eventoCodigo));
                p.Add(new NpgsqlParameter("@aluno",  matricula));
            });
    }

    public void RemoverParticipante(int eventoCodigo, int matricula)
    {
        ExecutarComando(
            "DELETE FROM TBEVENTOALUNO WHERE EVECODIGO = @evento AND ALUNMATRICULA = @aluno",
            p =>
            {
                p.Add(new NpgsqlParameter("@evento", eventoCodigo));
                p.Add(new NpgsqlParameter("@aluno",  matricula));
            });
    }

    public IEnumerable<Aluno> ObterParticipantes(int eventoCodigo)
    {
        return ConsultarComo(
            @"SELECT a.ALUNMATRICULA, a.ALUNNOME
                FROM TBALUNO a
                INNER JOIN TBEVENTOALUNO ea ON ea.ALUNMATRICULA = a.ALUNMATRICULA
               WHERE ea.EVECODIGO = @codigo
               ORDER BY a.ALUNNOME",
            MapearAlunoResumido,
            p => p.Add(new NpgsqlParameter("@codigo", eventoCodigo)));
    }

    public IEnumerable<Aluno> ObterNaoParticipantes(int eventoCodigo)
    {
        return ConsultarComo(
            @"SELECT a.ALUNMATRICULA, a.ALUNNOME
                FROM TBALUNO a
               WHERE a.ALUNSTATUS = 0
                 AND a.ALUNMATRICULA NOT IN (
                     SELECT ALUNMATRICULA FROM TBEVENTOALUNO WHERE EVECODIGO = @codigo)
               ORDER BY a.ALUNNOME",
            MapearAlunoResumido,
            p => p.Add(new NpgsqlParameter("@codigo", eventoCodigo)));
    }

    // ── helpers ────────────────────────────────────────────────

    /// <summary>Apenas matrícula e nome (suficiente para listas de participantes).</summary>
    private static Aluno MapearAlunoResumido(NpgsqlDataReader reader) => new()
    {
        Matricula = reader.GetInt32(0),
        Nome      = reader.GetString(1)
    };

    private static int ParaInteiro(DateTime data) => int.Parse(data.ToString("yyyyMMdd"));

    private static DateTime ConverterData(int dataYyyyMmDd) =>
        DateTime.ParseExact(dataYyyyMmDd.ToString(), "yyyyMMdd", null);
}
