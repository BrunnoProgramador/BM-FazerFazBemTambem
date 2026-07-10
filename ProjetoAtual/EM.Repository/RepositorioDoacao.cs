using EM.Domain;
using EM.Domain.Interfaces;
using Npgsql;

namespace EM.Repository;

public class RepositorioDoacao : RepositorioAbstrato<Doacao>, IRepositorioDoacao
{
    public RepositorioDoacao(string connectionString) : base(connectionString) { }

    public void Adicionar(Doacao entidade)
    {
        ExecutarComando(
            "INSERT INTO TBDOACAO (DOACODIGO, DOAITEM, DOAQTD, DOADATA, DOADOADOR) " +
            "VALUES (nextval('gen_tbdoacao'), @item, @qtd, @data, @doador)",
            p => PreencherParametros(p, entidade));
    }

    public override void Atualizar(Doacao entidade)
    {
        ExecutarComando(
            "UPDATE TBDOACAO SET DOAITEM = @item, DOAQTD = @qtd, DOADATA = @data, " +
            "DOADOADOR = @doador WHERE DOACODIGO = @codigo",
            p =>
            {
                PreencherParametros(p, entidade);
                p.Add(new NpgsqlParameter("@codigo", entidade.Codigo));
            });
    }

    public override void Excluir(int codigo)
    {
        ExecutarComando(
            "DELETE FROM TBDOACAO WHERE DOACODIGO = @codigo",
            p => p.Add(new NpgsqlParameter("@codigo", codigo)));
    }

    public override IEnumerable<Doacao> ObterTodos()
    {
        return Consultar(
            "SELECT DOACODIGO, DOAITEM, DOAQTD, DOADATA, DOADOADOR FROM TBDOACAO " +
            "ORDER BY DOADATA DESC, DOACODIGO DESC",
            Mapear);
    }

    public Doacao? GetByCodigo(int codigo)
    {
        return Consultar(
            "SELECT DOACODIGO, DOAITEM, DOAQTD, DOADATA, DOADOADOR FROM TBDOACAO " +
            "WHERE DOACODIGO = @codigo",
            Mapear,
            p => p.Add(new NpgsqlParameter("@codigo", codigo))).FirstOrDefault();
    }

    // ── helpers ────────────────────────────────────────────────

    private static void PreencherParametros(NpgsqlParameterCollection p, Doacao entidade)
    {
        p.Add(new NpgsqlParameter("@item",   entidade.Item));
        p.Add(new NpgsqlParameter("@qtd",    (object?)entidade.Quantidade ?? DBNull.Value));
        p.Add(new NpgsqlParameter("@data",   int.Parse(entidade.Data.ToString("yyyyMMdd"))));
        p.Add(new NpgsqlParameter("@doador", (object?)entidade.Doador ?? DBNull.Value));
    }

    private static Doacao Mapear(NpgsqlDataReader reader) => new()
    {
        Codigo     = reader.GetInt32(0),
        Item       = reader.GetString(1),
        Quantidade = reader.IsDBNull(2) ? null : reader.GetInt32(2),
        Data       = DateTime.ParseExact(reader.GetInt32(3).ToString(), "yyyyMMdd", null),
        Doador     = reader.IsDBNull(4) ? null : reader.GetString(4)
    };
}
