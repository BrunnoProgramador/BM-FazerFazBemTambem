namespace EM.Domain.Validadores;

public static class ValidadorVoluntario
{
    public static ResultadoValidacao Validar(this Voluntario voluntario)
    {
        if (string.IsNullOrWhiteSpace(voluntario.Nome))
            return ResultadoValidacao.Erro("Nome é obrigatório.");

        if (voluntario.Nome.Trim().Length < 2)
            return ResultadoValidacao.Erro("Nome deve ter no mínimo 2 caracteres.");

        if (voluntario.Nome.Trim().Length > 100)
            return ResultadoValidacao.Erro("Nome deve ter no máximo 100 caracteres.");

        if (voluntario.Telefone?.Trim().Length > 20)
            return ResultadoValidacao.Erro("Telefone deve ter no máximo 20 caracteres.");

        if (voluntario.Funcao?.Trim().Length > 50)
            return ResultadoValidacao.Erro("Função deve ter no máximo 50 caracteres.");

        return ResultadoValidacao.Ok();
    }
}
