using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Npgsql;

namespace EM.Web.Infraestrutura;

/// <summary>
/// Persiste as chaves do DataProtection (criptografia de cookies e
/// antiforgery) no próprio Postgres. Sem isso, cada instância serverless
/// do Vercel gera chaves novas e os logins caem aleatoriamente.
/// A tabela é criada automaticamente na primeira subida — nenhum script
/// manual precisa ser rodado no Neon.
/// </summary>
public class RepositorioChavesDataProtection : IXmlRepository
{
    private readonly string _connectionString;

    public RepositorioChavesDataProtection(string connectionString)
    {
        _connectionString = connectionString;
        GarantirTabela();
    }

    private void GarantirTabela()
    {
        using var conexao = new NpgsqlConnection(_connectionString);
        conexao.Open();
        using var comando = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS chaves_dataprotection (
                id   SERIAL PRIMARY KEY,
                nome VARCHAR(200),
                xml  TEXT NOT NULL
            )", conexao);
        comando.ExecuteNonQuery();
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        var elementos = new List<XElement>();

        using var conexao = new NpgsqlConnection(_connectionString);
        conexao.Open();
        using var comando = new NpgsqlCommand(
            "SELECT xml FROM chaves_dataprotection", conexao);
        using var reader = comando.ExecuteReader();

        while (reader.Read())
            elementos.Add(XElement.Parse(reader.GetString(0)));

        return elementos;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        using var conexao = new NpgsqlConnection(_connectionString);
        conexao.Open();
        using var comando = new NpgsqlCommand(
            "INSERT INTO chaves_dataprotection (nome, xml) VALUES (@nome, @xml)", conexao);
        comando.Parameters.AddWithValue("@nome", (object?)friendlyName ?? DBNull.Value);
        comando.Parameters.AddWithValue("@xml", element.ToString(SaveOptions.DisableFormatting));
        comando.ExecuteNonQuery();
    }
}
