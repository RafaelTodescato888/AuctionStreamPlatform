using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Register.Response;

namespace User.Domain.Interfaces.Kafka.Producers.Register;

public interface IRegisterBidderProducer
{
    Task<OneOf<bool, BaseError>> ProduceAsync(
        ResponseGenerateUserVerificationDTO request,
        string email,
        CancellationToken cancellationToken = default);
}
