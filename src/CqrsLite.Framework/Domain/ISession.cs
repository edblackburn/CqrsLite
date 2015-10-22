using System;

namespace CqrsLite.Framework.Domain
{
    public interface ISession
    {
        void Add<T>(T aggregate) where T : AggregateRoot;

        T Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot;

        void Commit();
    }
}