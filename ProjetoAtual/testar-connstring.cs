#:package Npgsql@9.0.3
using Npgsql;

// Lê a connection string real de uma variável de ambiente (não fica no histórico do bash)
var connStr = Environment.GetEnvironmentVariable("TESTE_CONN");
if (string.IsNullOrWhiteSpace(connStr))
{
    Console.WriteLine("Defina a variável TESTE_CONN antes de rodar. Exemplo:");
    Console.WriteLine("  export TESTE_CONN=\"Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull;Channel Binding=Require;\"");
    return;
}

Console.WriteLine($"Tamanho total da string: {connStr.Length} caracteres");
Console.WriteLine();

// 1) Testa a string inteira primeiro
try
{
    var builder = new NpgsqlConnectionStringBuilder(connStr);
    Console.WriteLine("✅ A string INTEIRA foi interpretada com sucesso. O problema não está na formatação.");
    Console.WriteLine($"   Host lido: {builder.Host}");
    Console.WriteLine($"   Database lido: {builder.Database}");
    Console.WriteLine($"   Username lido: {builder.Username}");
    return;
}
catch (Exception ex)
{
    Console.WriteLine("❌ A string inteira FALHOU: " + ex.Message);
    Console.WriteLine();
}

// 2) Bisseção: reconstrói clausula por clausula (separadas por ';') e testa incrementalmente
var clausulas = connStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
Console.WriteLine($"Encontradas {clausulas.Length} cláusulas separadas por ';'. Testando incrementalmente:");
Console.WriteLine();

var acumulado = "";
foreach (var clausula in clausulas)
{
    var candidato = acumulado + clausula.Trim() + ";";
    try
    {
        _ = new NpgsqlConnectionStringBuilder(candidato);
        Console.WriteLine($"  OK   -> depois de adicionar: \"{Ofuscar(clausula)}\"");
        acumulado = candidato;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  FALHA -> ao adicionar: \"{Ofuscar(clausula)}\"");
        Console.WriteLine($"           Erro: {ex.Message}");
        Console.WriteLine();
        Console.WriteLine("Cláusula problemática identificada (valor ofuscado acima por segurança).");
        break;
    }
}

static string Ofuscar(string clausula)
{
    var partes = clausula.Split('=', 2);
    if (partes.Length != 2) return clausula;
    var chave = partes[0].Trim();
    var valor = partes[1];
    if (chave.Equals("Password", StringComparison.OrdinalIgnoreCase))
        return $"{chave}=***(tamanho {valor.Length}, primeiro char: '{(valor.Length > 0 ? valor[0] : ' ')}', ultimo char: '{(valor.Length > 0 ? valor[^1] : ' ')}')";
    return clausula;
}
