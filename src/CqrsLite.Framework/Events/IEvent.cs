using System;
using CqrsLite.Framework.Messages;

namespace CqrsLite.Framework.Events
{
    public interface IEvent : IMessage
    {
        Guid Id { get; set; }
        int Version { get; set; }
        DateTimeOffset TimeStamp { get; set; }
    }
}

