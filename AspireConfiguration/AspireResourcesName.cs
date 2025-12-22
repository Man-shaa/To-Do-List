namespace AspireConfiguration;

public static class AspireResourcesName
{
    public const string TodoDatabase = "todo-db";

    private const string Todo = "todo";
    public const string TodoApi = $"{Todo}-api";
    public const string Postgres = "postgres";
    
    public const string Kafka = "kafka";

    public const string KafkaUiContainerName = "kafka-ui";

    public static class Dapr
    {
        public const string TodoConsumers = $"{Todo}.consumers";
        public const string KafkaPubSubName = "kafka-pubsub";
        public const string KafkaPubSubCreateTodoDirPath = "./dapr";
    }
}
