using EM.Domain;
using EM.Domain.Interfaces;
using EM.Domain.Utilitarios;
using Npgsql;

namespace EM.Repository;

public class RepositorioEncontro : RepositorioAbstrato<Encontro>, IRepositorioEncontro
{
    private static bool _migrado;
    private static readonly object _travaMigracao = new();

    public RepositorioEncontro(string connectionString) : base(connectionString)
    {
        GarantirColunaConteudo();
    }

    /// <summary>Automigração (uma vez por processo): coluna de conteúdo do encontro.</summary>
    private void GarantirColunaConteudo()
    {
        if (_migrado) return;
        lock (_travaMigracao)
        {
            if (_migrado) return;

            using var conexao = CriarConexao();
            using var comando = conexao.CreateCommand();
            comando.CommandText =
                "ALTER TABLE TBENCONTRO ADD COLUMN IF NOT EXISTS ENCCONTEUDO VARCHAR(500)";
            comando.ExecuteNonQuery();

            _migrado = true;
        }
    }

    public void Adicionar(Encontro entidade)
    {
        using var conexao = CriarConexao();
        using var transacao = conexao.BeginTransaction();

        int codigo;
        using (var comando = new NpgsqlCommand(
            "INSERT INTO TBENCONTRO (ENCCODIGO, ENCTURMA, ENCDATA, ENCCONTEUDO) " +
            "VALUES (nextval('gen_tbencontro'), @turma, @data, @conteudo) RETURNING ENCCODIGO",
            conexao, transacao))
        {
            comando.Parameters.Add(new NpgsqlParameter("@turma", entidade.Turma.Codigo));
            comando.Parameters.Add(new NpgsqlParameter("@data",  ParaInteiro(entidade.Data)));
            comando.Parameters.Add(new NpgsqlParameter("@conteudo",
                string.IsNullOrWhiteSpace(entidade.Conteudo) ? DBNull.Value : entidade.Conteudo.Trim()));
            codigo = Convert.ToInt32(comando.ExecuteScalar());
        }

        InserirPresencas(conexao, transacao, codigo, entidade.Presencas);
        transacao.Commit();
    }

    public override void Atualizar(Encontro entidade)
    {
        using var conexao = CriarConexao();
        using var transacao = conexao.BeginTransaction();

        using (var comando = new NpgsqlCommand(
            "UPDATE TBENCONTRO SET ENCDATA = @data, ENCCONTEUDO = @conteudo WHERE ENCCODIGO = @codigo",
            conexao, transacao))
        {
            comando.Parameters.Add(new NpgsqlParameter("@data",   ParaInteiro(entidade.Data)));
            comando.Parameters.Add(new NpgsqlParameter("@conteudo",
                string.IsNullOrWhiteSpace(entidade.Conteudo) ? DBNull.Value : entidade.Conteudo.Trim()));
            comando.Parameters.Add(new NpgsqlParameter("@codigo", entidade.Codigo));
            comando.ExecuteNonQuery();
        }

        using (var comando = new NpgsqlCommand(
            "DELETE FROM TBENCPRESENCA WHERE ENCCODIGO = @codigo", conexao, transacao))
        {
            comando.Parameters.Add(new NpgsqlParameter("@codigo", entidade.Codigo));
            comando.ExecuteNonQuery();
        }

        InserirPresencas(conexao, transacao, entidade.Codigo, entidade.Presencas);
        transacao.Commit();
    }

    public override void Excluir(int codigo)
    {
        using var conexao = CriarConexao();
        using var transacao = conexao.BeginTransaction();

        foreach (var sql in new[]
        {
            "DELETE FROM TBENCPRESENCA WHERE ENCCODIGO = @codigo",
            "DELETE FROM TBENCONTRO WHERE ENCCODIGO = @codigo"
        })
        {
            using var comando = new NpgsqlCommand(sql, conexao, transacao);
            comando.Parameters.Add(new NpgsqlParameter("@codigo", codigo));
            comando.ExecuteNonQuery();
        }

        transacao.Commit();
    }

    public override IEnumerable<Encontro> ObterTodos()
    {
        return Consultar(
            @"SELECT e.ENCCODIGO, e.ENCDATA, e.ENCCONTEUDO, t.TURMCODIGO, t.TURMNOME, t.TURMAANO,
                     COUNT(p.ALUNMATRICULA) AS TOTAL,
                     COALESCE(SUM(p.PRESENTE), 0) AS PRESENTES
                FROM TBENCONTRO e
                INNER JOIN TBTURMA t ON t.TURMCODIGO = e.ENCTURMA
                LEFT JOIN TBENCPRESENCA p ON p.ENCCODIGO = e.ENCCODIGO
               GROUP BY e.ENCCODIGO, e.ENCDATA, e.ENCCONTEUDO, t.TURMCODIGO, t.TURMNOME, t.TURMAANO
               ORDER BY e.ENCDATA DESC, e.ENCCODIGO DESC",
            reader => new Encontro
            {
                Codigo   = reader.GetInt32(0),
                Data     = ConverterData(reader.GetInt32(1)),
                Conteudo = reader.IsDBNull(2) ? null : reader.GetString(2),
                Turma    = new Turma
                {
                    Codigo = reader.GetInt32(3),
                    Nome   = reader.GetString(4),
                    Ano    = reader.GetInt32(5)
                },
                TotalAlunos    = reader.GetInt32(6),
                TotalPresentes = reader.GetInt32(7)
            });
    }

    public Encontro? GetByCodigo(int codigo)
    {
        var encontro = Consultar(
            @"SELECT e.ENCCODIGO, e.ENCDATA, e.ENCCONTEUDO, t.TURMCODIGO, t.TURMNOME, t.TURMAANO
                FROM TBENCONTRO e
                INNER JOIN TBTURMA t ON t.TURMCODIGO = e.ENCTURMA
               WHERE e.ENCCODIGO = @codigo",
            reader => new Encontro
            {
                Codigo   = reader.GetInt32(0),
                Data     = ConverterData(reader.GetInt32(1)),
                Conteudo = reader.IsDBNull(2) ? null : reader.GetString(2),
                Turma    = new Turma
                {
                    Codigo = reader.GetInt32(3),
                    Nome   = reader.GetString(4),
                    Ano    = reader.GetInt32(5)
                }
            },
            p => p.Add(new NpgsqlParameter("@codigo", codigo))).FirstOrDefault();

        if (encontro == null) return null;

        encontro.Presencas = ConsultarComo(
            @"SELECT p.ALUNMATRICULA, a.ALUNNOME, p.PRESENTE
                FROM TBENCPRESENCA p
                INNER JOIN TBALUNO a ON a.ALUNMATRICULA = p.ALUNMATRICULA
               WHERE p.ENCCODIGO = @codigo
               ORDER BY a.ALUNNOME",
            reader => new PresencaAluno
            {
                Aluno = new Aluno
                {
                    Matricula = reader.GetInt32(0),
                    Nome      = reader.GetString(1)
                },
                Presente = reader.GetInt32(2) == 1
            },
            p => p.Add(new NpgsqlParameter("@codigo", codigo))).ToList();

        encontro.TotalAlunos    = encontro.Presencas.Count;
        encontro.TotalPresentes = encontro.Presencas.Count(x => x.Presente);
        return encontro;
    }

    public IEnumerable<RegistroPresencaAluno> ObterPresencasDoAluno(int matricula)
    {
        return ConsultarComo(
            @"SELECT e.ENCDATA, p.PRESENTE, t.TURMNOME
                FROM TBENCPRESENCA p
                INNER JOIN TBENCONTRO e ON e.ENCCODIGO = p.ENCCODIGO
                INNER JOIN TBTURMA t ON t.TURMCODIGO = e.ENCTURMA
               WHERE p.ALUNMATRICULA = @matricula
               ORDER BY e.ENCDATA DESC, e.ENCCODIGO DESC",
            reader => new RegistroPresencaAluno
            {
                Data      = ConverterData(reader.GetInt32(0)),
                Presente  = reader.GetInt32(1) == 1,
                TurmaNome = reader.GetString(2)
            },
            p => p.Add(new NpgsqlParameter("@matricula", matricula)));
    }

    public IEnumerable<ResumoPresencaAluno> ObterResumoPorAluno()
    {
        return ConsultarComo(
            @"SELECT a.ALUNMATRICULA, a.ALUNNOME, a.ALUNSTATUS,
                     COUNT(p.ENCCODIGO) AS TOTAL,
                     COALESCE(SUM(p.PRESENTE), 0) AS PRESENCAS,
                     MAX(CASE WHEN p.PRESENTE = 1 THEN e.ENCDATA END) AS ULTIMA
                FROM TBALUNO a
                LEFT JOIN TBENCPRESENCA p ON p.ALUNMATRICULA = a.ALUNMATRICULA
                LEFT JOIN TBENCONTRO e ON e.ENCCODIGO = p.ENCCODIGO
               GROUP BY a.ALUNMATRICULA, a.ALUNNOME, a.ALUNSTATUS
               ORDER BY a.ALUNNOME",
            reader => new ResumoPresencaAluno
            {
                Matricula      = reader.GetInt32(0),
                Nome           = reader.GetString(1),
                StatusCodigo   = reader.GetInt32(2),
                TotalEncontros = reader.GetInt32(3),
                TotalPresencas = reader.GetInt32(4),
                UltimaPresenca = reader.IsDBNull(5) ? null : ConverterData(reader.GetInt32(5))
            });
    }

    // ── helpers ────────────────────────────────────────────────

    private static void InserirPresencas(
        NpgsqlConnection conexao, NpgsqlTransaction transacao, int encontroCodigo, List<PresencaAluno> presencas)
    {
        foreach (var presenca in presencas)
        {
            using var comando = new NpgsqlCommand(
                "INSERT INTO TBENCPRESENCA (ENCCODIGO, ALUNMATRICULA, PRESENTE) " +
                "VALUES (@encontro, @aluno, @presente)",
                conexao, transacao);
            comando.Parameters.Add(new NpgsqlParameter("@encontro", encontroCodigo));
            comando.Parameters.Add(new NpgsqlParameter("@aluno",    presenca.Aluno.Matricula));
            comando.Parameters.Add(new NpgsqlParameter("@presente", presenca.Presente ? 1 : 0));
            comando.ExecuteNonQuery();
        }
    }

    private static int ParaInteiro(DateTime data) => int.Parse(data.ToString("yyyyMMdd"));

    private static DateTime ConverterData(int dataYyyyMmDd) =>
        DateTime.ParseExact(dataYyyyMmDd.ToString(), "yyyyMMdd", null);
}
