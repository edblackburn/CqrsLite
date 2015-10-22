using System;

namespace CqrsLite.Framework.Snapshots
{
    public abstract class Snapshot
    {
        public Guid Id { get; set; }

        public int Version { get; set; }
    }
}
