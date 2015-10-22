using System;
using CqrsLite.Framework.Domain;
using CqrsLite.Framework.Snapshots;
using CqrsLite.Tests.Substitutes;
using NUnit.Framework;

namespace CqrsLite.Tests.Snapshots
{
	[TestFixture]
    public class When_getting_snapshotable_aggreate
    {
        private TestSnapshotStore _snapshotStore;
        private TestSnapshotAggregate _aggregate;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestInMemoryEventStore();
            var eventPublisher = new TestEventPublisher();
            _snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
		    var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(eventStore, eventPublisher), eventStore);
            var session = new Session(repository);

            _aggregate = session.Get<TestSnapshotAggregate>(Guid.NewGuid());
        }

        [Test]
        public void Should_ask_for_snapshot()
        {
            Assert.True(_snapshotStore.VerifyGet);
        }

        [Test]
        public void Should_run_restore_method()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}
