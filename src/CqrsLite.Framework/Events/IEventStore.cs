using System;
using System.Collections.Generic;

namespace CqrsLite.Framework.Events
{
    public interface IEventStore 
    {
        void Save(Type aggregateType, IEvent @event);

        IEnumerable<IEvent> Get(Type aggregateType, Guid aggregateId, int fromVersion);
    }
}