using Confluent.Kafka;
using System.Text.Json;

namespace CrossCutting.Kafka.Common.Serialization;

internal sealed class KafkaJsonDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(
        ReadOnlySpan<byte> data,
        bool isNull,
        SerializationContext context)
    {
        if (isNull)
            return default!;

        return JsonSerializer.Deserialize<T>(data)!;
    }
}

