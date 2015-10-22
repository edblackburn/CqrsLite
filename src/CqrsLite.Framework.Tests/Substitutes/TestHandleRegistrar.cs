using System;
using System.Collections.Generic;
using CqrsLite.Framework.Bus;
using CqrsLite.Framework.Messages;

namespace CqrsLite.Tests.Substitutes
{
    public class TestHandleRegistrar : IHandlerRegistrar
    {
        public static readonly IList<TestHandlerListItem> HandlerList = new List<TestHandlerListItem>();

        public void RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            HandlerList.Add(new TestHandlerListItem {Type = typeof(T),Handler = handler});
        }
    }

    public class TestHandlerListItem
    {
        public Type Type;
        public dynamic Handler;
    }
}
