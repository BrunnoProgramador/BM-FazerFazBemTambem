using EM.Domain;
using EM.Domain.Interfaces;
using EM.Domain.Utilitarios;
using Npgsql;

namespace EM.Repository;

public class RepositorioTurma : IRepositorioTurma
{
    private readonly string _connectionString;

    public RepositorioTurma(string connectionString)
    {
        _connectionString = connectionString;
    }

    private NpgsqlConnection CriarConexao()
    {
        var conexao = new NpgsqlConnection(_connectionString);
        conexao.Open();
        return conexao;
    }

    private void ExecutarComando(string sql, Action<NpgsqlParameterCollection> parametros)
    {
        using var conexao = CriarConexao();
        using var comando = new NpgsqlCommand(sql, conexao);
        parametros(comando.Parameters);
        comando.ExecuteNonQuery();
    }

    public void Adicionar(Turma entidade)
    {
        ExecutarComando(
            "INSERT INTO TBTURMA (TURMCODIGO, TURMNOME, TURMAANO, TURMPROFESSOR, TURMDIAS, TURMHORARIO) " +
            "VALUES (nextval('gen_tbturma'), @nome, @ano, @professor, @dias, @horario)",
            p =>
            {
                p.Add(new NpgsqlParameter("@nome", entidade.Nome));
                p.Add(new NpgsqlParameter("@ano",  entidade.Ano));
                p.Add(new NpgsqlParameter("@professor", (object?)entidade.Professor  ?? DBNull.Value));
                p.Add(new NpgsqlParameter("@dias",      (object?)entidade.DiasSemana ?? DBNull.Value));
                p.Add(new NpgsqlParameter("@horario",   (object?)entidade.Horario    ?? DBNull.Value));
            });
    }

    public void Atualizar(Turma entidade)
    {
        ExecutarComando(
            "UPDATE TBTURMA SET TURMNOME = @nome, TURMAANO = @ano, TURMPROFESSOR = @professor, " +
            "TURMDIAS = @dias, TURMHORARIO = @horario WHERE TURMCODIGO = @codigo",
            p =>
            {
                p.Add(new NpgsqlParameter("@nome",   entidade.Nome));
                p.Add(new NpgsqlParameter("@ano",    entidade.Ano));
                p.Add(new NpgsqlParameter("@professor", (object?)entidade.Professor  ?? DBNull.Value));
                p.Add(new NpgsqlParameter("@dias",      (object?)entidade.DiasSemana ?? DBNull.Value));
                p.Add(new NpgsqlParameter("@horario",   (object?)entidade.Horario    ?? DBNull.Value));
                p.Add(new NpgsqlParameter("@codigo", entidade.Codigo));
            });
    }

    public void Excluir(int codigo)
    {
        ExecutarComando(
            "DELETE FROM TBTURMA_ALUNO WHERE TURMCODIGO = @codigo",
            p => p.Add(new NpgsqlParameter("@codigo", codigo)));
        ExecutarComando(
            "DELETE FROM TBTURMA WHERE TURMCODIGO = @codigo",
            p => p.Add(new NpgsqlParameter("@codigo", codigo)));
    }

    public IEnumerable<Turma> ObterTodos()
    {
        using var conexao = CriarConexao();
        using var cmd = new NpgsqlCommand(
            @"SELECT t.TURMCODIGO, t.TURMNOME, t.TURMAANO, t.TURMPROFESSOR, t.TURMDIAS, t.TURMHORARIO,
                     COUNT(ta.ALUNMATRICULA) AS TOTAL
              FROM TBTURMA t
              LEFT JOIN TBTURMA_ALUNO ta ON ta.TURMCODIGO = t.TURMCODIGO
              GROUP BY t.TURMCODIGO, t.TURMNOME, t.TURMAANO, t.TURMPROFESSOR, t.TURMDIAS, t.TURMHORARIO
              ORDER BY t.TURMAANO DESC, t.TURMNOME", conexao);
        using var reader = cmd.ExecuteReader();
        var lista = new List<Turma>();
        while (reader.Read())
            lista.Add(new Turma
            {
                Codigo      = reader.GetInt32(0),
                Nome        = reader.GetString(1),
                Ano         = reader.GetInt32(2),
                Professor   = reader.IsDBNull(3) ? null : reader.GetString(3),
                DiasSemana  = reader.IsDBNull(4) ? null : reader.GetString(4),
                Horario     = reader.IsDBNull(5) ? null : reader.GetString(5),
                TotalAlunos = reader.GetInt32(6)
            });
        return lista;
    }

    public Turma? GetByCodigo(int codigo)
    {
        using var conexao = CriarConexao();
        using var cmd = new NpgsqlCommand(
            @"SELECT t.TURMCODIGO, t.TURMNOME, t.TURMAANO, t.TURMPROFESSOR, t.TURMDIAS, t.TURMHORARIO,
                     (SELECT COUNT(*) FROM TBTURMA_ALUNO ta WHERE ta.TURMCODIGO = t.TURMCODIGO) AS TOTAL
              FROM TBTURMA t
              WHERE t.TURMCODIGO = @codigo",
            conexao);
        cmd.Parameters.Add(new NpgsqlParameter("@codigo", codigo));
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return new Turma
        {
            Codigo      = reader.GetInt32(0),
            Nome        = reader.GetString(1),
            Ano         = reader.GetInt32(2),
            Professor   = reader.IsDBNull(3) ? null : reader.GetString(3),
            DiasSemana  = reader.IsDBNull(4) ? null : reader.GetString(4),
            Horario     = reader.IsDBNull(5) ? null : reader.GetString(5),
            TotalAlunos = reader.GetInt32(6)
        };
    }

    public void AdicionarAluno(int turmaCodigo, int matricula)
    {
        ExecutarComando(
            "INSERT INTO TBTURMA_ALUNO (TURMCODIGO, ALUNMATRICULA) VALUES (@turma, @aluno)",
            p =>
            {
                p.Add(new NpgsqlParameter("@turma", turmaCodigo));
                p.Add(new NpgsqlParameter("@aluno", matricula));
            });
    }

    public void RemoverAluno(int turmaCodigo, int matricula)
    {
        ExecutarComando(
            "DELETE FROM TBTURMA_ALUNO WHERE TURMCODIGO = @turma AND ALUNMATRICULA = @aluno",
            p =>
            {
                p.Add(new NpgsqlParameter("@turma", turmaCodigo));
                p.Add(new NpgsqlParameter("@aluno", matricula));
            });
    }

    public IEnumerable<Aluno> ObterAlunosDaTurma(int turmaCodigo)
    {
        using var conexao = CriarConexao();
        using var cmd = new NpgsqlCommand(
            @"SELECT a.ALUNMATRICULA, a.ALUNNOME, a.ALUNSTATUS
              FROM TBALUNO a
              INNER JOIN TBTURMA_ALUNO ta ON ta.ALUNMATRICULA = a.ALUNMATRICULA
              WHERE ta.TURMCODIGO = @codigo
              ORDER BY a.ALUNNOME", conexao);
        cmd.Parameters.Add(new NpgsqlParameter("@codigo", turmaCodigo));
        using var reader = cmd.ExecuteReader();
        var lista = new List<Aluno>();
        while (reader.Read()) lista.Add(MapearAluno(reader));
        return lista;
    }

    public IEnumerable<Aluno> ObterAlunosForaDaTurma(int turmaCodigo)
    {
        // Apenas alunos ativos podem ser adicionados a uma turma.
        using var conexao = CriarConexao();
        using var cmd = new NpgsqlCommand(
            @"SELECT a.ALUNMATRICULA, a.ALUNNOME, a.ALUNSTATUS
              FROM TBALUNO a
              WHERE a.ALUNSTATUS = 0
                AND a.ALUNMATRICULA NOT IN (
                  SELECT ALUNMATRICULA FROM TBTURMA_ALUNO WHERE TURMCODIGO = @codigo
              )
              ORDER BY a.ALUNNOME", conexao);
        cmd.Parameters.Add(new NpgsqlParameter("@codigo", turmaCodigo));
        using var reader = cmd.ExecuteReader();
        var lista = new List<Aluno>();
        while (reader.Read()) lista.Add(MapearAluno(reader));
        return lista;
    }

    /// <summary>Matrícula, nome e status (suficiente para as listas da turma).</summary>
    private static Aluno MapearAluno(NpgsqlDataReader reader) => new()
    {
        Matricula = reader.GetInt32(0),
        Nome      = reader.GetString(1),
        Status    = EnumeradorStatusAluno.ObtenhaPorCodigo(reader.GetInt32(2))
    };
}
