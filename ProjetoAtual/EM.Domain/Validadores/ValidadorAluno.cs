using System.Text.RegularExpressions;

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

        if (!string.IsNullOrEmpty(aluno.CPF) && !CpfValido(aluno.CPF))
            return ResultadoValidacao.Erro("CPF inválido.");

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

    private static bool CpfValido(string cpf)
    {
        cpf = Regex.Replace(cpf, @"\D", "");

        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;

        int soma1 = cpf.Take(9).Select((c, i) => (c - '0') * (10 - i)).Sum();
        int dig1  = soma1 % 11 < 2 ? 0 : 11 - soma1 % 11;
        if (dig1 != cpf[9] - '0') return false;

        int soma2 = cpf.Take(10).Select((c, i) => (c - '0') * (11 - i)).Sum();
        int dig2  = soma2 % 11 < 2 ? 0 : 11 - soma2 % 11;
        return dig2 == cpf[10] - '0';
    }
}
