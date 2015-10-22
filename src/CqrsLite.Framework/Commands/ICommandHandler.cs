using CqrsLite.Framework.Messages;

namespace CqrsLite.Framework.Commands
{
	public interface ICommandHandler<in T> : IHandler<T> where T : ICommand
	{
	}
}