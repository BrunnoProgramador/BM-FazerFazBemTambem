namespace EM.Domain.Validadores;

public static class ValidadorEncontro
{
    public static ResultadoValidacao Validar(this Encontro encontro)
    {
        if (encontro.Turma == null || encontro.Turma.Codigo <= 0)
            return ResultadoValidacao.Erro("Turma é obrigatória.");

        if (encontro.Data == default)
            return ResultadoValidacao.Erro("Data é obrigatória.");

        if (encontro.Data.Date > DateTime.Today)
            return ResultadoValidacao.Erro("Data não pode ser futura.");

        if (encontro.Presencas.Count == 0)
            return ResultadoValidacao.Erro("A turma não possui alunos para registrar presença.");

        if (encontro.Conteudo?.Trim().Length > 500)
            return ResultadoValidacao.Erro("Conteúdo deve ter no máximo 500 caracteres.");

        return ResultadoValidacao.Ok();
    }
}
