using System;
using System.Collections.Generic;
using CqrsLite.Framework.Domain.Exception;
using CqrsLite.Framework.Events;
using CqrsLite.Framework.Infrastructure;

namespace CqrsLite.Framework.Domain
{
    public abstract class AggregateRoot
    {
        readonly List<IEvent> _changes = new List<IEvent>();

        public Guid Id { get; protected set; }
        public int Version { get; protected set; }

        public IEnumerable<IEvent> GetUncommittedChanges()
        {
            lock (_changes)
            {
                return _changes.ToArray();
            }
        }

        public void MarkChangesAsCommitted()
        {
            lock(_changes)
            {
                Version = Version + _changes.Count;
                _changes.Clear();
            }
        }

        public void LoadFromHistory(IEnumerable<IEvent> history)
        {
            foreach (var e in history)
            {
                if (e.Version != Version + 1)
                    throw new EventsOutOfOrderException(e.Id);
                ApplyChange(e, false);
            }
        }

        protected void ApplyChange(IEvent @event)
        {
            ApplyChange(@event, true);
        }

        void ApplyChange(IEvent @event, bool isNew)
        {
            lock (_changes)
            {
                this.AsDynamic().Apply(@event);
                if (isNew)
                {
                    _changes.Add(@event);
                }
                else
                {
                    Id = @event.Id;
                    Version++;
                }
            }
        }
    }
}