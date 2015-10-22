using System;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Domain.Exception;
using CqrsLite.Tests.Substitutes;
using NUnit.Framework;

namespace CqrsLite.Tests.Domain
{
	[TestFixture]
    public class When_getting_aggregate_without_contructor
    {
	    private ISession _session;

	    [SetUp]
        public void Setup()
        {
            var eventStore = new TestInMemoryEventStore();
            var eventPublisher = new TestEventPublisher();
            _session = new Session(new Repository(eventStore, eventPublisher));
        }

        [Test]
        public void Should_throw_missing_parameterless_constructor_exception()
        {
            Assert.Throws<MissingParameterLessConstructorException>(() => _session.Get<TestAggregateNoParameterLessConstructor>(Guid.NewGuid()));
        }
    }
}