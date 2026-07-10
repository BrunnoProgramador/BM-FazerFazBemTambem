using Xunit;

namespace EM.Testes.Integracao;

/// <summary>
/// Teste que só roda quando a variável de ambiente EM_TESTE_FIREBIRD
/// contém a connection string de um banco Firebird DE TESTE.
/// Sem a variável, o teste aparece como "Skipped" — assim o
/// `dotnet test` passa em qualquer máquina e no CI sem banco.
///
/// Exemplo (PowerShell):
///   $env:EM_TESTE_FIREBIRD="DataSource=localhost;Port=3055;Database=C:\temp\TESTE.FB5;User=SYSDBA;Password=masterkey;Charset=UTF8"
///   dotnet test
/// </summary>
public sealed class FatoIntegracaoAttribute : FactAttribute
{
    public FatoIntegracaoAttribute()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            Skip = "Defina EM_TESTE_FIREBIRD com a connection string de um banco de TESTE para rodar.";
    }

    public static string? ConnectionString =>
        Environment.GetEnvironmentVariable("EM_TESTE_FIREBIRD");
}
