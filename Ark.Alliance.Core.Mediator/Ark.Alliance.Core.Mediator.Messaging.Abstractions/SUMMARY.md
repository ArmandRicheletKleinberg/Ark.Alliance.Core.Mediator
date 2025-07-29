# Ark.Alliance.Core.Mediator.Messaging.Abstractions API Summary

| Type | Summary |
| ---- | ------- |
| `IBrokerProducer` | Publishes messages to an external broker. |
| `IBrokerConsumer` | Subscribes handlers to messages from an external broker. |
| `IBrokerOptions` | Marker interface for broker configuration options. |
| `BrokerMetadata` | Contains topic and headers describing a broker message. |
| `IStreamPublisher<T>` | Streams items to a broker with acknowledgements. |
| `IStreamSubscriber<T>` | Consumes items from a broker topic. |
| `Ack` | Indicates whether message publication succeeded. |
| `IArkBrokerRegistrar` | Registers broker services from configuration. |
