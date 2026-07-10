using EM.Domain.Interface;

namespace EM.Domain;

public class Voluntario : IEntidade
{
    public int Codigo { get; set; }
    public string Nome { get; set; }
    public string? Telefone { get; set; }

    /// <summary>Função no projeto (Professor, Auxiliar, Cozinha, Recepção...).</summary>
    public string? Funcao { get; set; }
}
