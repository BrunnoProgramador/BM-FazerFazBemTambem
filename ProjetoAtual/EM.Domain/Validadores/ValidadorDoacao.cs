namespace EM.Domain.Validadores;

public static class ValidadorDoacao
{
    public static ResultadoValidacao Validar(this Doacao doacao)
    {
        if (string.IsNullOrWhiteSpace(doacao.Item))
            return ResultadoValidacao.Erro("Item doado é obrigatório.");

        if (doacao.Item.Trim().Length > 150)
            return ResultadoValidacao.Erro("Item deve ter no máximo 150 caracteres.");

        if (doacao.Quantidade is <= 0)
            return ResultadoValidacao.Erro("Quantidade deve ser maior que zero.");

        if (doacao.Data == default)
            return ResultadoValidacao.Erro("Data é obrigatória.");

        if (doacao.Data.Date > DateTime.Today)
            return ResultadoValidacao.Erro("Data não pode ser futura.");

        if (doacao.Doador?.Trim().Length > 100)
            return ResultadoValidacao.Erro("Nome do doador deve ter no máximo 100 caracteres.");

        return ResultadoValidacao.Ok();
    }
}
