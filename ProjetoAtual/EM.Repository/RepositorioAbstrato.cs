using EM.Domain.Interface;
using Npgsql;

namespace EM.Repository;

public abstract class RepositorioAbstrato<T> where T : IEntidade
{

    protected readonly string _connectionString;

    protected RepositorioAbstrato(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected NpgsqlConnection CriarConexao()
    {
        var conexao = new NpgsqlConnection(_connectionString);
        conexao.Open();
        return conexao;
    }

    protected void ExecutarComando(string sql, Action<NpgsqlParameterCollection> parametros)
    {
        using var conexao = CriarConexao();
        using var comando = new NpgsqlCommand(sql, conexao);
        parametros(comando.Parameters);
        comando.ExecuteNonQuery();
    }

    protected object? ExecutarEscalar(string sql, Action<NpgsqlParameterCollection> parametros)
    {
        using var conexao = CriarConexao();
        using var comando = new NpgsqlCommand(sql, conexao);
        parametros(comando.Parameters);
        return comando.ExecuteScalar();
    }

    protected IEnumerable<TResultado> ConsultarComo<TResultado>(
        string sql,
        Func<NpgsqlDataReader, TResultado> mapear,
        Action<NpgsqlParameterCollection>? parametros = null)
    {
        using var conexao = CriarConexao();
        using var comando = new NpgsqlCommand(sql, conexao);
        parametros?.Invoke(comando.Parameters);
        using var reader = comando.ExecuteReader();
        var lista = new List<TResultado>();
        while (reader.Read())
            lista.Add(mapear(reader));
        return lista;
    }

    protected IEnumerable<T> Consultar(
        string sql,
        Func<NpgsqlDataReader, T> mapear,
        Action<NpgsqlParameterCollection>? parametros = null)
    {
        return ConsultarComo(sql, mapear, parametros);
    }

    public abstract void Atualizar(T entidade);
    public abstract void Excluir(int codigo);
    public abstract IEnumerable<T> ObterTodos();

}
