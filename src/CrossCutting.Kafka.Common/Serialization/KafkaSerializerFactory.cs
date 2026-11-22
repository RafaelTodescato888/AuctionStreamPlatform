using Confluent.Kafka;

namespace CrossCutting.Kafka.Common.Serialization;

internal static class KafkaSerializerFactory
{
    public static ISerializer<T> Create<T>()
    {
        if (typeof(T) == typeof(string))
            return (ISerializer<T>)Serializers.Utf8;

        if (typeof(T) == typeof(byte[]))
            return (ISerializer<T>)Serializers.ByteArray;

        return new KafkaJsonSerializer<T>();
    }
}