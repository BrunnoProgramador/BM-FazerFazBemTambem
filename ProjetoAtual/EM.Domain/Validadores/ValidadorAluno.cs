namespace EM.Domain.Validadores;

public static class ValidadorAluno
{
    public static ResultadoValidacao Validar(this Aluno aluno)
    {
        if (string.IsNullOrWhiteSpace(aluno.Nome))
            return ResultadoValidacao.Erro("Nome é obrigatório.");

        if (aluno.Nome.Trim().Length < 3)
            return ResultadoValidacao.Erro("Nome deve ter no mínimo 3 caracteres.");

        if (aluno.Nome.Trim().Length > 100)
            return ResultadoValidacao.Erro("Nome deve ter no máximo 100 caracteres.");

        if (aluno.Nascimento == default)
            return ResultadoValidacao.Erro("Data de nascimento é obrigatória.");

        if (aluno.Nascimento.Date > DateTime.Today)
            return ResultadoValidacao.Erro("Data de nascimento não pode ser futura.");

        if (aluno.Nascimento.Date < DateTime.Today.AddYears(-120))
            return ResultadoValidacao.Erro("Data de nascimento inválida.");

        if (aluno.Cidade?.Trim().Length > 100)
            return ResultadoValidacao.Erro("Cidade deve ter no máximo 100 caracteres.");

        if (aluno.Bairro?.Trim().Length > 100)
            return ResultadoValidacao.Erro("Bairro deve ter no máximo 100 caracteres.");

        if (aluno.ResponsavelNome?.Trim().Length > 100)
            return ResultadoValidacao.Erro("Nome do responsável deve ter no máximo 100 caracteres.");

        if (aluno.ResponsavelTelefone?.Trim().Length > 20)
            return ResultadoValidacao.Erro("Telefone do responsável deve ter no máximo 20 caracteres.");

        if (aluno.ResponsavelParentesco?.Trim().Length > 50)
            return ResultadoValidacao.Erro("Parentesco deve ter no máximo 50 caracteres.");

        return ResultadoValidacao.Ok();
    }
}
