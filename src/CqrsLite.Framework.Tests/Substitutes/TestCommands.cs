using System;
using CqrsLite.Framework.Commands;

namespace CqrsLite.Tests.Substitutes
{
    public class TestAggregateDoSomething : ICommand
    {
        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
    }

    public class TestAggregateDoSomethingHandler : ICommandHandler<TestAggregateDoSomething> 
    {
        public void Handle(TestAggregateDoSomething message)
        {
            TimesRun++;
        }

        public int TimesRun { get; set; }
    }
	public class TestAggregateDoSomethingElseHandler : ICommandHandler<TestAggregateDoSomething>
    {
        public void Handle(TestAggregateDoSomething message)
        {

        }
    }
}