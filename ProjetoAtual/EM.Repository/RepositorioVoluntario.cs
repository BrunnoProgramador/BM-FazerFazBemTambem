using EM.Domain;
using EM.Domain.Interfaces;
using Npgsql;

namespace EM.Repository;

public class RepositorioVoluntario : RepositorioAbstrato<Voluntario>, IRepositorioVoluntario
{
    public RepositorioVoluntario(string connectionString) : base(connectionString) { }

    public void Adicionar(Voluntario entidade)
    {
        ExecutarComando(
            "INSERT INTO TBVOLUNTARIO (VOLCODIGO, VOLNOME, VOLTELEFONE, VOLFUNCAO) " +
            "VALUES (nextval('gen_tbvoluntario'), @nome, @telefone, @funcao)",
            p => PreencherParametros(p, entidade));
    }

    public override void Atualizar(Voluntario entidade)
    {
        ExecutarComando(
            "UPDATE TBVOLUNTARIO SET VOLNOME = @nome, VOLTELEFONE = @telefone, " +
            "VOLFUNCAO = @funcao WHERE VOLCODIGO = @codigo",
            p =>
            {
                PreencherParametros(p, entidade);
                p.Add(new NpgsqlParameter("@codigo", entidade.Codigo));
            });
    }

    public override void Excluir(int codigo)
    {
        ExecutarComando(
            "DELETE FROM TBVOLUNTARIO WHERE VOLCODIGO = @codigo",
            p => p.Add(new NpgsqlParameter("@codigo", codigo)));
    }

    public override IEnumerable<Voluntario> ObterTodos()
    {
        return Consultar(
            "SELECT VOLCODIGO, VOLNOME, VOLTELEFONE, VOLFUNCAO FROM TBVOLUNTARIO ORDER BY VOLNOME",
            Mapear);
    }

    public Voluntario? GetByCodigo(int codigo)
    {
        return Consultar(
            "SELECT VOLCODIGO, VOLNOME, VOLTELEFONE, VOLFUNCAO FROM TBVOLUNTARIO " +
            "WHERE VOLCODIGO = @codigo",
            Mapear,
            p => p.Add(new NpgsqlParameter("@codigo", codigo))).FirstOrDefault();
    }

    // ── helpers ────────────────────────────────────────────────

    private static void PreencherParametros(NpgsqlParameterCollection p, Voluntario entidade)
    {
        p.Add(new NpgsqlParameter("@nome",     entidade.Nome));
        p.Add(new NpgsqlParameter("@telefone", (object?)entidade.Telefone ?? DBNull.Value));
        p.Add(new NpgsqlParameter("@funcao",   (object?)entidade.Funcao   ?? DBNull.Value));
    }

    private static Voluntario Mapear(NpgsqlDataReader reader) => new()
    {
        Codigo   = reader.GetInt32(0),
        Nome     = reader.GetString(1),
        Telefone = reader.IsDBNull(2) ? null : reader.GetString(2),
        Funcao   = reader.IsDBNull(3) ? null : reader.GetString(3)
    };
}
