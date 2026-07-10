namespace EM.Domain.Utilitarios;

/// <summary>
/// Situação do aluno no projeto. Substitui a exclusão de cadastro
/// no dia a dia: em vez de apagar, muda-se o status.
/// </summary>
public class EnumeradorStatusAluno
{
    public readonly int Codigo;
    public readonly string Descricao;

    public static readonly EnumeradorStatusAluno Ativo             = new(0, "Ativo");
    public static readonly EnumeradorStatusAluno Inativo           = new(1, "Inativo");
    public static readonly EnumeradorStatusAluno MudouDeCidade     = new(2, "Mudou de cidade");
    public static readonly EnumeradorStatusAluno ParouDeFrequentar = new(3, "Parou de frequentar");

    public static readonly IReadOnlyList<EnumeradorStatusAluno> Todos =
        new[] { Ativo, Inativo, MudouDeCidade, ParouDeFrequentar };

    private EnumeradorStatusAluno(int codigo, string descricao)
    {
        Codigo = codigo;
        Descricao = descricao;
    }

    public static EnumeradorStatusAluno ObtenhaPorCodigo(int codigo) =>
        Todos.FirstOrDefault(s => s.Codigo == codigo) ?? Ativo;
}
