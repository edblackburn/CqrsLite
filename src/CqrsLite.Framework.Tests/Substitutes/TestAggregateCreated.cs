﻿using System;
using CqrsLite.Framework.Events;

namespace CqrsLite.Tests.Substitutes
{
    public class TestAggregateCreated : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}