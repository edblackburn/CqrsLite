﻿using System;
using System.Collections.Generic;
using CqrsLite.Framework.Bus;
using CqrsLite.Framework.Config;

namespace CqrsLite.Tests.Substitutes
{
    public class TestServiceLocator : IServiceLocator
    {
        public readonly List<dynamic> Handlers = new List<dynamic>();
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type type)
        {
            if(type == typeof(IHandlerRegistrar))
                return new TestHandleRegistrar();
            if (type == typeof(TestAggregateDidSomethingHandler))
            {
                var handler = new TestAggregateDidSomethingHandler();
                Handlers.Add(handler);
                return handler;
            }
            else
            {
                var handler = new TestAggregateDoSomethingHandler();
                Handlers.Add(handler);
                return handler;
            }
        }
    }
}