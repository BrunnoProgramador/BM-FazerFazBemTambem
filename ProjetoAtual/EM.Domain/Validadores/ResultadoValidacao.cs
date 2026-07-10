namespace EM.Domain.Validadores;

public class ResultadoValidacao
{
    public bool Valido { get; }
    public string Mensagem { get; }

    private ResultadoValidacao(bool valido, string mensagem)
    {
        Valido = valido;
        Mensagem = mensagem;
    }

    public static ResultadoValidacao Ok() => new(true, string.Empty);
    public static ResultadoValidacao Erro(string mensagem) => new(false, mensagem);
}
