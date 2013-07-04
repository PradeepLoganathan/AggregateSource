using AggregateSource.GEventStore.Resolvers;

namespace AggregateSource.GEventStore.Framework {
  public static class EventStoreReadConfigurationFactory {
    public static EventStoreReadConfiguration Create() {
      return new EventStoreReadConfiguration(new SliceSize(1), new EventDeserializer(), new PassThroughStreamNameResolver());
    }

    public static EventStoreReadConfiguration CreateWithResolver(IStreamNameResolver resolver) {
      return new EventStoreReadConfiguration(new SliceSize(1), new EventDeserializer(), resolver);
    }
  }
}