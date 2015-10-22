using CqrsLite.Framework.Messages;

namespace CqrsLite.Framework.Commands
{
    public interface ICommand : IMessage
    {
        int ExpectedVersion { get; set; }
    }
}