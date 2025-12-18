using Tuat.Interfaces;

namespace Tuat.LibraryEditor.Sandbox;

internal class MessageBusService(LibraryTester _libraryTester) : IMessageBusService
{
    public ValueTask PublishAsync<T>(T message)
    {
        return ValueTask.CompletedTask;
    }
}
