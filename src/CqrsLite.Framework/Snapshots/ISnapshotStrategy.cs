using System;
using CqrsLite.Framework.Domain;

namespace CqrsLite.Framework.Snapshots
{
    public interface ISnapshotStrategy
    {
        bool ShouldMakeSnapShot(AggregateRoot aggregate);
        bool IsSnapshotable(Type aggregateType);
    }
}