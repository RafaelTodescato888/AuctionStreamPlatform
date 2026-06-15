using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Login.Response;

namespace User.Domain.Interfaces.UseCase.Login.Commands
{
    public interface ILoginUseCase
    {
        Task<OneOf<ResponseUserLoginDTO, BaseError>> LoginAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default);
    }
}
