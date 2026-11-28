using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Register.Request;

namespace User.Domain.Interfaces.Kafka.Producers.Register;

public interface IRegisterSellerProducer
{
    Task<OneOf<bool, BaseError>> ProduceAsync(RequestSellerCreationProducerDTO request, CancellationToken cancellationToken = default);
}
