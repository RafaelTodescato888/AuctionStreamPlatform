using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Update.Request;

namespace User.Domain.Interfaces.UseCase.Update.Commands;

public interface IUpdateProfileUserUseCase
{
    Task<OneOf<bool, BaseError>> UpdateAsync(RequestUpdateProfileUserDTO request, CancellationToken cancellationToken);
}
