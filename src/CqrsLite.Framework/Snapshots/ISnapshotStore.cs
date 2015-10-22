using System;

namespace CqrsLite.Framework.Snapshots
{
    public interface ISnapshotStore
    {
        Snapshot Get(Guid id);

        void Save(Snapshot snapshot);
    }
}
