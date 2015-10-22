﻿using System;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Domain.Exception;
using CqrsLite.Tests.Substitutes;
using NUnit.Framework;

namespace CqrsLite.Tests.Domain
{
    [TestFixture]
    public class When_saving_stale_data
    {
        private TestInMemoryEventStore _eventStore;
        private TestAggregate _aggregate;
        private TestEventPublisher _eventPublisher;
        private Repository _rep;
        private Session _session;

        [SetUp]
        public void Setup()
        {
            _eventStore = new TestInMemoryEventStore();
            _eventPublisher = new TestEventPublisher();
            _rep = new Repository(_eventStore, _eventPublisher);
            _session = new Session(_rep);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _aggregate.DoSomething();
            _rep.Save(_aggregate);
        }

        [Test]
        public void Should_throw_concurrency_exception_from_repository()
        {
            Assert.Throws<ConcurrencyException>(() => _rep.Save(_aggregate, 0));
        }

        [Test]
        public void Should_throw_concurrency_exception_from_session()
        {
            _session.Add(_aggregate);
            _aggregate.DoSomething();
            _rep.Save(_aggregate);
            Assert.Throws<ConcurrencyException>(() => _session.Commit());
        }
    }
}