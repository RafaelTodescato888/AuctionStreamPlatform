using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Register.Request;

namespace User.Domain.Interfaces.UseCase.Register.Bidder.Commands;

public interface IRegisterBidderUseCase
{
    Task<OneOf<bool, BaseError>> RegisterAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default);
}
