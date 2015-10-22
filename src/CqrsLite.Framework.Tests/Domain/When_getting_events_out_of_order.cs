using System;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Domain.Exception;
using CqrsLite.Tests.Substitutes;
using NUnit.Framework;

namespace CqrsLite.Tests.Domain
{
	[TestFixture]
    public class When_getting_events_out_of_order
    {
	    private ISession _session;

	    [SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStoreWithBugs();
            var testEventPublisher = new TestEventPublisher();
            _session = new Session(new Repository(eventStore, testEventPublisher));
        }

        [Test]
        public void Should_throw_concurrency_exception()
        {
            var id = Guid.NewGuid();
            Assert.Throws<EventsOutOfOrderException>(() => _session.Get<TestAggregate>(id, 3));
        }
    }
}