using System;
using System.Linq;
using CqrsLite.Framework.Domain.Exception;
using CqrsLite.Framework.Domain.Factories;
using CqrsLite.Framework.Events;

namespace CqrsLite.Framework.Domain
{
    public class Repository : IRepository
    {
        readonly IEventStore _eventStore;
        readonly IEventPublisher _publisher;

        public Repository(IEventStore eventStore, IEventPublisher publisher)
        {
            if(eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            if(publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            _eventStore = eventStore;
            _publisher = publisher;
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            if (expectedVersion != null && _eventStore.Get(typeof(T), aggregate.Id, expectedVersion.Value).Any())
                throw new ConcurrencyException(aggregate.Id);

            var i = 0;
            foreach (var @event in aggregate.GetUncommittedChanges())
            {
                if (@event.Id == Guid.Empty) 
                    @event.Id = aggregate.Id;

                if (@event.Id == Guid.Empty)
                    throw new AggregateOrEventMissingIdException(aggregate.GetType(), @event.GetType());

                i++;

                @event.Version = aggregate.Version + i;
                @event.TimeStamp = DateTimeOffset.UtcNow;
                _eventStore.Save(typeof(T), @event);
                _publisher.Publish(@event);
            }
            aggregate.MarkChangesAsCommitted();
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            return LoadAggregate<T>(aggregateId);
        }

        T LoadAggregate<T>(Guid id) where T : AggregateRoot
        {
            var aggregate = AggregateFactory.CreateAggregate<T>();

            var events = _eventStore.Get(typeof(T), id, -1).ToArray();
            if (!events.Any())
                throw new AggregateNotFoundException(id);

            aggregate.LoadFromHistory(events);
            return aggregate;
        }
    }
}