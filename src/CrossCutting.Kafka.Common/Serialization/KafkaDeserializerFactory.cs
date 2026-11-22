using Confluent.Kafka;

namespace CrossCutting.Kafka.Common.Serialization;

internal static class KafkaDeserializerFactory
{
    public static IDeserializer<T> Create<T>()
    {
        var type = typeof(T);

        if (type == typeof(string))
            return (IDeserializer<T>)Deserializers.Utf8;

        if (type == typeof(byte[]))
            return (IDeserializer<T>)Deserializers.ByteArray;

        return new KafkaJsonDeserializer<T>();
    }
}