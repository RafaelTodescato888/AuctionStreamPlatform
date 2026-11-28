using Microsoft.Extensions.DependencyInjection;
using User.Application.UseCases.Register.Bidder.Commands;
using User.Application.UseCases.Register.Seller.Commands;
using User.Application.UseCases.Register.Verification.Commands;
using User.Domain.Interfaces.UseCase.Register.Bidder.Commands;
using User.Domain.Interfaces.UseCase.Register.Seller.Commands;
using User.Domain.Interfaces.UseCase.Register.Verification.Commands;

namespace User.Application.Extensions.UseCases.Register;

internal static class UseCaseExtensions
{
    internal static IServiceCollection AddRegisterUseCases(this IServiceCollection services)
    {
        services.AddTransient<IRegisterBidderUseCase, RegisterBidderUseCase>();
        services.AddTransient<IRegisterSellerUseCase, RegisterSellerUseCase>();
        services.AddTransient<IGenerateUserVerificationUseCase, GenerateUserVerificationUseCase>();

        return services;
    }
}
