using CrossCutting.Errors.Base;
using CrossCutting.Kafka.Common.Producers.Contracts;
using OneOf;
using User.Domain.Dto.Register.Response;
using User.Domain.Interfaces.Kafka.Producers.Register;
using User.Infrastructure.Kafka.Envelopes.Register;

namespace User.Infrastructure.Kafka.Producers.Register;

public class RegisterBidderProducer(
    IProducer<BidderUserCreatedEnvelope> producer
) : IRegisterBidderProducer
{
    public async Task<OneOf<bool, BaseError>> ProduceAsync(
        ResponseGenerateUserVerificationDTO request, string email, CancellationToken cancellationToken = default)
    {
        var envelope = new BidderUserCreatedEnvelope
        {
            Key = Guid.NewGuid().ToString(),
            Value = new(request.UserId, email, request.VerificationType, request.Code, request.ExpiresAt)
        };

        await producer.ProduceAsync(envelope, cancellationToken);

        return true;
    }
}
