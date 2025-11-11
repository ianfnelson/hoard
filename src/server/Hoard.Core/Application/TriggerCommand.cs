namespace Hoard.Core.Application;

public interface ITriggerCommand : ICommand
{
    object ToBusCommand();
}