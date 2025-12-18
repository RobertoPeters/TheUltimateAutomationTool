using Tuat.Interfaces;

namespace Tuat.LibraryEditor.Sandbox;

internal class MessageBusService(LibraryTester _libraryTester) : IMessageBusService
{
    public ValueTask PublishAsync<T>(T message)
    {
        Task.Factory.StartNew(async () =>
            {
                if (message is List<VariableInfo> variableInfos)
                {
                    if (_libraryTester.ClientService != null)
                    {
                        await _libraryTester.ClientService.Handle(variableInfos);
                    }
                    _libraryTester.UIEventRegistration?.Handle(variableInfos);
                }
                else if (message is List<VariableValueInfo> variableValueInfos)
                {
                    _libraryTester.UIEventRegistration?.Handle(variableValueInfos);
                }
                else if (message is LogEntry logEntry)
                {
                    _libraryTester.UIEventRegistration?.Handle(logEntry);
                }
            });
        return ValueTask.CompletedTask;
    }
}
