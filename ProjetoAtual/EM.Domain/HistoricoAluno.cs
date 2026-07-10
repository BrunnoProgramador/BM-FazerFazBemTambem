using EM.Domain.Interface;

namespace EM.Domain;

/// <summary>
/// Linha do tempo do aluno no projeto. Registrado automaticamente
/// pelo sistema (cadastro, mudanças de status, turmas, eventos).
/// </summary>
public class HistoricoAluno : IEntidade
{
    public int Codigo { get; set; }
    public int AlunoMatricula { get; set; }
    public DateTime Data { get; set; }
    public string Texto { get; set; }
}
