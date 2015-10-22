﻿using System;
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
        static readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

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
                    _locks.TryRemove(x.CacheItem.Key, out o);
                }
            };
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            var idstring = aggregate.Id.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    if (aggregate.Id != Guid.Empty && !IsTracked(aggregate.Id))
                        _cache.Add(idstring, aggregate, _policyFactory.Invoke());
                    _repository.Save(aggregate, expectedVersion);
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.Remove(idstring);
                }
                throw;
            }
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var idstring = aggregateId.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    T aggregate;
                    if (IsTracked(aggregateId))
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
                    _cache.Add(aggregateId.ToString(), aggregate, _policyFactory.Invoke());
                    return aggregate;
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.Remove(idstring);
                }
                throw;
            }
        }

        bool IsTracked(Guid id)
        {
            return _cache.Contains(id.ToString());
        }
    }
}