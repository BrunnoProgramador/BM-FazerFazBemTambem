namespace EM.Domain.Validadores;

public static class ValidadorEvento
{
    public static ResultadoValidacao Validar(this Evento evento)
    {
        if (string.IsNullOrWhiteSpace(evento.Nome))
            return ResultadoValidacao.Erro("Nome do evento é obrigatório.");

        if (evento.Nome.Trim().Length < 2)
            return ResultadoValidacao.Erro("Nome deve ter no mínimo 2 caracteres.");

        if (evento.Nome.Trim().Length > 100)
            return ResultadoValidacao.Erro("Nome deve ter no máximo 100 caracteres.");

        if (evento.Data == default)
            return ResultadoValidacao.Erro("Data é obrigatória.");

        if (evento.Observacao?.Trim().Length > 200)
            return ResultadoValidacao.Erro("Observação deve ter no máximo 200 caracteres.");

        return ResultadoValidacao.Ok();
    }
}
