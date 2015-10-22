using CqrsLite.Framework.Messages;

namespace CqrsLite.Framework.Events
{
	public interface IEventHandler<T> : IHandler<T> where T : IEvent
	{
	}
}