using CrossCutting.Errors.Base;
using OneOf;
using System.Security.Cryptography;
using User.Domain.Errors.Register;
using User.Domain.Interfaces.Services.Authentication.Register;

namespace User.Infrastructure.Services.Authentication.Register;

public class GenerateNumberCodeService : IGenerateNumberCodeService
{
    public OneOf<string, BaseError> GenerateNumberCodeAsync(int numberSize = 6)
    {
        if (numberSize < 0)
            return new CodeSizeShouldntBeNegative();

        var numberCode = new char[numberSize];
        for (var index = 0; index < numberCode.Length; index++)
        {
            numberCode[index] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(numberCode);
    }
}
