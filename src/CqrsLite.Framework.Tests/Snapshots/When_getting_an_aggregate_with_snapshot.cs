using System;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Snapshots;
using CqrsLite.Tests.Substitutes;
using NUnit.Framework;

namespace CqrsLite.Tests.Snapshots
{
	[TestFixture]
    public class When_getting_an_aggregate_with_snapshot
    {
        private TestSnapshotAggregate _aggregate;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestInMemoryEventStore();
            var eventPublisher = new TestEventPublisher();
            var snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
		    var snapshotRepository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore, eventPublisher), eventStore);
            var session = new Session(snapshotRepository);

            _aggregate = session.Get<TestSnapshotAggregate>(Guid.NewGuid());
        }

        [Test]
        public void Should_restore()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}
