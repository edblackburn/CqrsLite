﻿using System;
using System.Linq;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Domain.Exception;
using CqrsLite.Tests.Substitutes;
using NUnit.Framework;

namespace CqrsLite.Tests.Domain
{
	[TestFixture]
    public class When_saving
    {
        private TestInMemoryEventStore _eventStore;
        private TestAggregateNoParameterLessConstructor _aggregate;
        private TestEventPublisher _eventPublisher;
	    private ISession _session;
	    private Repository _rep;

	    [SetUp]
        public void Setup()
        {
            _eventStore = new TestInMemoryEventStore();
            _eventPublisher = new TestEventPublisher();
	        _rep = new Repository(_eventStore, _eventPublisher);
            _session = new Session(_rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);
        }

        [Test]
        public void Should_save_uncommited_changes()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(1, _eventStore.Events.Count);
        }

        [Test]
        public void Should_mark_commited_after_commit()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(0, _aggregate.GetUncommittedChanges().Count());
        }
        
        [Test]
        public void Should_publish_events()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(1, _eventPublisher.Published);
        }

        [Test]
        public void Should_add_new_aggregate()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.AreEqual(1, _eventStore.Events.Count);
        }

        [Test]
        public void Should_set_date()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.That(_eventStore.Events.First().TimeStamp, Is.InRange(DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1)));
        }

        [Test]
        public void Should_set_version()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.That(_eventStore.Events.First().Version, Is.EqualTo(1));
            Assert.That(_eventStore.Events.Last().Version, Is.EqualTo(2));

        }

        [Test]
        public void Should_set_id()
        {
            var id = Guid.NewGuid();
            var agg = new TestAggregateNoParameterLessConstructor(1, id);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.That(_eventStore.Events.First().Id, Is.EqualTo(id));
        }

        [Test]
        public void Should_clear_tracked_aggregates()
        {
            var agg = new TestAggregate(Guid.NewGuid());
            _session.Add(agg);
            agg.DoSomething();
            _session.Commit();
            _eventStore.Events.Clear();

            Assert.Throws<AggregateNotFoundException>(() => _session.Get<TestAggregate>(agg.Id));
        }
    }
}
