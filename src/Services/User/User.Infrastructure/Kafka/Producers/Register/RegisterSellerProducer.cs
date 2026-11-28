using CrossCutting.Errors.Base;
using CrossCutting.Kafka.Common.Producers.Contracts;
using OneOf;
using User.Domain.Dto.Register.Request;
using User.Domain.Interfaces.Kafka.Producers.Register;
using User.Infrastructure.Kafka.Envelopes.Register;

namespace User.Infrastructure.Kafka.Producers.Register;

public class RegisterSellerProducer(
    IProducer<SellerUserCreatedEnvelope> producer
) : IRegisterSellerProducer
{
    public async Task<OneOf<bool, BaseError>> ProduceAsync(RequestSellerCreationProducerDTO request, CancellationToken cancellationToken = default)
    {
        var key = Guid.NewGuid();
        var envelope = new SellerUserCreatedEnvelope
        {
            Key = key.ToString(),
            Value = new(key, request.OccurredAt, request.UserId, request.SellerApplicationId, request.Name, request.Email)
        };

        await producer.ProduceAsync(envelope, cancellationToken);

        return true;
    }
} 
