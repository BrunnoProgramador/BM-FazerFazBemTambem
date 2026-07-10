namespace EM.Domain.Validadores;

public static class ValidadorTurma
{
    public static ResultadoValidacao Validar(this Turma turma)
    {
        if (string.IsNullOrWhiteSpace(turma.Nome))
            return ResultadoValidacao.Erro("Nome da turma é obrigatório.");

        if (turma.Nome.Trim().Length < 2)
            return ResultadoValidacao.Erro("Nome deve ter no mínimo 2 caracteres.");

        if (turma.Nome.Trim().Length > 100)
            return ResultadoValidacao.Erro("Nome deve ter no máximo 100 caracteres.");

        if (turma.Ano < 2000 || turma.Ano > 2100)
            return ResultadoValidacao.Erro("Ano inválido.");

        if (turma.Professor?.Trim().Length > 100)
            return ResultadoValidacao.Erro("Nome do professor deve ter no máximo 100 caracteres.");

        if (turma.DiasSemana?.Trim().Length > 60)
            return ResultadoValidacao.Erro("Dias da semana devem ter no máximo 60 caracteres.");

        if (turma.Horario?.Trim().Length > 20)
            return ResultadoValidacao.Erro("Horário deve ter no máximo 20 caracteres.");

        return ResultadoValidacao.Ok();
    }
}
