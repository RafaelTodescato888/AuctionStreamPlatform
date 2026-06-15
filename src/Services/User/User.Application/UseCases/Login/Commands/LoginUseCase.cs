using CrossCutting.Errors.Base;
using CrossCutting.Extensions;
using OneOf;
using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Login.Response;
using User.Domain.Enums;
using User.Domain.Errors.Login;
using User.Domain.Interfaces.Services.Authentication.Login;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Login.Commands;

namespace User.Application.UseCases.Login.Commands
{
    internal sealed class LoginUseCase(
        IUnitOfWork unitOfWork,
        IPasswordHashService passwordHashService,
        IGenerateTokenService generateTokenService
    ) : ILoginUseCase
    {
        public async Task<OneOf<ResponseUserLoginDTO, BaseError>> LoginAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default)
        {
            var hashPassword = passwordHashService.GenerateHash(content.Password);
            content.HashPassword(hashPassword);

            var userToLogin = await GetUserByCredentials(content, cancellationToken);
            if (userToLogin.IsError())
                return userToLogin.GetError();

            var userToLoginInformations = userToLogin.GetValue();
            var refreshTokenInformations = generateTokenService.GenerateRefreshToken();

            if (!await unitOfWork.UserRepository.RefreshTokenAsync(refreshTokenInformations, userToLoginInformations.Id, cancellationToken))
                return new DatabaseError();

            return new ResponseUserLoginDTO(generateTokenService.GenerateToken(userToLoginInformations), refreshTokenInformations.RefreshToken);
        }

        private async Task<OneOf<RequestGenerateTokenDTO, BaseError>> GetUserByCredentials(RequestUserLoginDTO content, CancellationToken cancellationToken = default)
        {
            var userToLogin = await unitOfWork.UserRepository.GetUserByCredentialsAsync(content, cancellationToken);
            if (userToLogin == null)
                return new UserOrPasswordIsIncorrectError();

            if (userToLogin.Status == EUserStatus.BANNED)
                return new UserIsBannedError();

            return userToLogin;
        }
    }
}
