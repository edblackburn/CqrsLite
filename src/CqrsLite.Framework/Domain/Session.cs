using System;
using System.Collections.Generic;
using CqrsLite.Framework.Domain.Exception;

namespace CqrsLite.Framework.Domain
{
    public class Session : ISession
    {
        readonly IRepository _repository;
        readonly Dictionary<AggregateKey, AggregateDescriptor> _trackedAggregates;

        public Session(IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            _repository = repository;
            _trackedAggregates = new Dictionary<AggregateKey, AggregateDescriptor>();
        }

        public void Add<T>(T aggregate) where T : AggregateRoot
        {
            var key = new AggregateKey(typeof(T), aggregate.Id);
            if (!IsTracked(key))
            {
                _trackedAggregates.Add(key, new AggregateDescriptor { Aggregate = aggregate, Version = aggregate.Version });
            }
            else if (_trackedAggregates[key].Aggregate != aggregate)
            {
                throw new ConcurrencyException(aggregate.Id);
            }
        }

        public T Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot
        {
            var key = new AggregateKey(typeof(T), id);

            if (IsTracked(key))
            {
                var trackedAggregate = (T)_trackedAggregates[key].Aggregate;
                if (expectedVersion != null && trackedAggregate.Version != expectedVersion)
                    throw new ConcurrencyException(trackedAggregate.Id);

                return trackedAggregate;
            }

            var aggregate = _repository.Get<T>(id);
            if (expectedVersion != null && aggregate.Version != expectedVersion)
                throw new ConcurrencyException(id);
            Add(aggregate);

            return aggregate;
        }

        bool IsTracked(AggregateKey key)
        {
            return _trackedAggregates.ContainsKey(key);
        }

        public void Commit()
        {
            foreach (var descriptor in _trackedAggregates.Values)
                _repository.Save(descriptor.Aggregate, descriptor.Version);

            _trackedAggregates.Clear();
        }

        class AggregateDescriptor
        {
            public AggregateRoot Aggregate { get; set; }
            public int Version { get; set; }
        }

        class AggregateKey
        {
            readonly Type _aggregateType;
            readonly Guid _aggregateId;

            internal AggregateKey(Type aggregateType, Guid aggregateId)
            {
                _aggregateType = aggregateType;
                _aggregateId = aggregateId;
            }

            public override bool Equals(object obj)
            {
                var that = obj as AggregateKey;
                if (that == null)
                    return false;

                return _aggregateId == that._aggregateId &&
                       _aggregateType == that._aggregateType;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_aggregateType?.GetHashCode() ?? 0) * 397) ^ _aggregateId.GetHashCode();
                }
            }
        }
    }
}
