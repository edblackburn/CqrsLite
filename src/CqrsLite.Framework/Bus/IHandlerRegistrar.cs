using System;
using CqrsLite.Framework.Messages;

namespace CqrsLite.Framework.Bus
{
    public interface IHandlerRegistrar
    {
        void RegisterHandler<T>(Action<T> handler) where T : IMessage;
    }
}