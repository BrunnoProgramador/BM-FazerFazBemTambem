namespace EM.Domain.Utilitarios;

public class EnumeradorSexo
{
    public readonly int Codigo;
    public readonly string Descricao;
    public readonly string DescricaoResumida;

    public static EnumeradorSexo Masculino = new(0, "Masculino", "M");
    public static EnumeradorSexo Feminino = new(1, "Feminino", "F");

    private EnumeradorSexo(int codigo, string descricao, string descricaoResumida)
    {
        Codigo = codigo;
        Descricao = descricao;
        DescricaoResumida = descricaoResumida;
    }

    public static EnumeradorSexo ObtenhaPorCodigo(int codigo)
    {
        if (Masculino.Codigo == codigo)
        {
            return Masculino;
        }

        return Feminino;

    }

}
