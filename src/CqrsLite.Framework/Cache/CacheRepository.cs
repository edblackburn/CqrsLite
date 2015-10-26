using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Caching;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Events;

namespace CqrsLite.Framework.Cache
{
    public class CacheRepository : IRepository
    {
        readonly IRepository _repository;
        readonly IEventStore _eventStore;
        readonly MemoryCache _cache;
        readonly Func<CacheItemPolicy> _policyFactory;
        static readonly ConcurrentDictionary<string, object> Locks = new ConcurrentDictionary<string, object>();

        public CacheRepository(IRepository repository, IEventStore eventStore)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            _repository = repository;
            _eventStore = eventStore;
            _cache = MemoryCache.Default;
            _policyFactory = () => new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(0, 0, 15, 0),
                RemovedCallback = x =>
                {
                    object o;
                    Locks.TryRemove(x.CacheItem.Key, out o);
                }
            };
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            var idstring = GetIdString(aggregate);

            try
            {
                lock (Locks.GetOrAdd(idstring, _ => new object()))
                {
                    if (aggregate.Id != Guid.Empty && !IsTracked(idstring))
                        _cache.Add(idstring, aggregate, _policyFactory.Invoke());

                    _repository.Save(aggregate, expectedVersion);
                }
            }
            catch (Exception)
            {
                lock (Locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.Remove(idstring);
                }
                throw;
            }
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var idstring = GetIdString(typeof (T), aggregateId);

            try
            {
                lock (Locks.GetOrAdd(idstring, _ => new object()))
                {
                    T aggregate;
                    if (IsTracked(idstring))
                    {
                        aggregate = (T)_cache.Get(idstring);
                        var events = _eventStore.Get(typeof(T), aggregateId, aggregate.Version);
                        if (events.Any() && events.First().Version != aggregate.Version + 1)
                        {
                            _cache.Remove(idstring);
                        }
                        else
                        {
                            aggregate.LoadFromHistory(events);
                            return aggregate;
                        }
                    }

                    aggregate = _repository.Get<T>(aggregateId);
                    _cache.Add(idstring, aggregate, _policyFactory.Invoke());
                    return aggregate;
                }
            }
            catch (Exception)
            {
                lock (Locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.Remove(idstring);
                }
                throw;
            }
        }

        static string GetIdString(Type type, Guid aggregateId)
        {
            return $"{type.FullName}-{aggregateId}";
        }

        static string GetIdString(AggregateRoot aggregate)
        {
            return GetIdString(aggregate.GetType(), aggregate.Id);
        }

        bool IsTracked(string aggregateIdString)
        {
            return _cache.Contains(aggregateIdString);
        }
    }
}