using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Errors.Register;

namespace User.Domain.Interfaces.Services.Authentication.Register;

public interface IGenerateNumberCodeService
{
    OneOf<string, BaseError> GenerateNumberCodeAsync(int numberSize = 6);
}
