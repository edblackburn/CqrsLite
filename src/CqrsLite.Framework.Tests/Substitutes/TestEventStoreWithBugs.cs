using System;
using System.Collections.Generic;
using CqrsLite.Framework.Events;

namespace CqrsLite.Tests.Substitutes
{
    public class TestEventStoreWithBugs : IEventStore
    {
        public IEnumerable<IEvent> Get(Type aggregateType, Guid aggregateId, int version)
        {
            if (aggregateId == Guid.Empty)
            {
                return new List<IEvent>();
            }

            return new List<IEvent>
                {
                    new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                    new TestAggregateDidSomething {Id = aggregateId, Version = 2},
                    new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 1},
                };
        }

        public void Save(Type aggregateType, IEvent eventDescriptor)
        {
        }
    }
}