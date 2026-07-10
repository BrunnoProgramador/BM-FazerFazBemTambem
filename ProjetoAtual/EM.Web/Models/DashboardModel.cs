namespace EM.Web.Models;

public class DashboardModel
{
    public int TotalAlunosAtivos { get; set; }
    public int TotalTurmas { get; set; }
    public int EncontrosRealizados { get; set; }

    /// <summary>Presença média geral (null quando não há encontros).</summary>
    public int? PresencaMedia { get; set; }

    /// <summary>Encontros registrados hoje (vazio = "frequência não registrada").</summary>
    public List<EncontroModel> EncontrosHoje { get; set; } = new();
}
