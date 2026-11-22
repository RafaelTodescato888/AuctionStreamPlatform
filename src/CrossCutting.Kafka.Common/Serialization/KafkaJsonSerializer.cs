using Confluent.Kafka;
using System.Text.Json;

namespace CrossCutting.Kafka.Common.Serialization;

internal sealed class KafkaJsonSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(
        T data,
        SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}