﻿using System;
using System.Linq;
using CqrsLite.Framework.Domain;

namespace CqrsLite.Framework.Snapshots
{
    public class DefaultSnapshotStrategy : ISnapshotStrategy
    {
        const int SnapshotInterval = 15;

        public bool IsSnapshotable(Type aggregateType)
        {
            if (aggregateType.BaseType == null)

                return false;
            if (aggregateType.BaseType.IsGenericType &&
                aggregateType.BaseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<>))
                return true;

            return IsSnapshotable(aggregateType.BaseType);
        }

        public bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            if (!IsSnapshotable(aggregate.GetType())) return false;
            var i = aggregate.Version;

            for (var j = 0; j < aggregate.GetUncommittedChanges().Count(); j++)
                if (++i % SnapshotInterval == 0 && i != 0) return true;
            return false;
        }
    }
}