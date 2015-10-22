using System;
using System.Collections.Generic;
using System.Linq;
using CqrsLite.Framework.Events;

namespace CqrsLite.Tests.Substitutes
{
    public class TestEventStore : IEventStore
    {
        readonly Guid _emptyGuid;

        public TestEventStore()
        {
            _emptyGuid = Guid.NewGuid();
            SavedEvents = new List<IEvent>();
        }

        public IEnumerable<IEvent> Get(Type aggregateType, Guid aggregateId, int version)
        {
            if (aggregateId == _emptyGuid || aggregateId == Guid.Empty)
            {
                return new List<IEvent>();
            }

            return new List<IEvent>
                {
                    new TestAggregateDidSomething {Id = aggregateId, Version = 1},
                    new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 2},
                    new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                }.Where(x => x.Version > version);
        }

        public void Save(Type aggregateType, IEvent @event)
        {
            SavedEvents.Add(@event);
        }

        private List<IEvent> SavedEvents { get; set; }
    }
}
