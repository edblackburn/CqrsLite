using System;
using System.Linq;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Domain.Factories;
using CqrsLite.Framework.Events;
using CqrsLite.Framework.Infrastructure;

namespace CqrsLite.Framework.Snapshots
{
    public class SnapshotRepository : IRepository
    {
        readonly ISnapshotStore _snapshotStore;
        readonly ISnapshotStrategy _snapshotStrategy;
        readonly IRepository _repository;
        readonly IEventStore _eventStore;

        public SnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, IRepository repository, IEventStore eventStore)
        {
            if(snapshotStore == null)
                throw new ArgumentNullException(nameof(snapshotStore));

            if (snapshotStrategy == null)
                throw new ArgumentNullException(nameof(snapshotStrategy));

            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            _snapshotStore = snapshotStore;
            _snapshotStrategy = snapshotStrategy;
            _repository = repository;
            _eventStore = eventStore;
        }

        public void Save<T>(T aggregate, int? exectedVersion = null) where T : AggregateRoot
        {
            TryMakeSnapshot(aggregate);
            _repository.Save(aggregate, exectedVersion);
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var aggregate = AggregateFactory.CreateAggregate<T>();
            var snapshotVersion = TryRestoreAggregateFromSnapshot(aggregateId, aggregate);
            if(snapshotVersion == -1)
            {
                return _repository.Get<T>(aggregateId);
            }
            var events = _eventStore.Get(typeof(T), aggregateId, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
            aggregate.LoadFromHistory(events);

            return aggregate;
        }

        int TryRestoreAggregateFromSnapshot<T>(Guid id, T aggregate)
        {
            var version = -1;
            if (_snapshotStrategy.IsSnapshotable(typeof(T)))
            {
                var snapshot = _snapshotStore.Get(id);
                if (snapshot != null)
                {
                    aggregate.AsDynamic().Restore(snapshot);
                    version = snapshot.Version;
                }
            }
            return version;
        }

        void TryMakeSnapshot(AggregateRoot aggregate)
        {
            if (!_snapshotStrategy.ShouldMakeSnapShot(aggregate))
                return;
            var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
            snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
            _snapshotStore.Save(snapshot);
        }
    }
}