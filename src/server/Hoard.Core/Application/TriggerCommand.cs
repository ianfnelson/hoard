namespace Hoard.Core.Application;

public interface ITriggerCommand : ICommand
{
    int RunTypeId { get; }
    Guid RunId { get; }
    object ToBusCommand();
}