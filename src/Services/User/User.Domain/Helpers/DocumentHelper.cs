namespace User.Domain.Helpers;

using System.Linq;

public static class DocumentHelper
{
    public static bool IsValidCpf(this string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if ((cpf[9] - '0') != firstDigit)
            return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return (cpf[10] - '0') == secondDigit;
    }

    public static bool IsValidCnpj(this string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

        if (cnpj.Length != 14)
            return false;

        if (cnpj.Distinct().Count() == 1)
            return false;

        int[] firstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] secondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (cnpj[i] - '0') * firstWeights[i];

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if ((cnpj[12] - '0') != firstDigit)
            return false;

        sum = 0;
        for (var i = 0; i < 13; i++)
            sum += (cnpj[i] - '0') * secondWeights[i];

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return (cnpj[13] - '0') == secondDigit;
    }
}
