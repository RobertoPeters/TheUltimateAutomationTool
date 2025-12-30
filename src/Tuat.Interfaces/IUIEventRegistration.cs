using Tuat.Models;

namespace Tuat.Interfaces;

public interface IUIEventRegistration
{
    event EventHandler<ClientHandlerInfo>? ClientHandlerChanged;
    event EventHandler<AutomationHandlerInfo>? AutomationHandlerChanged;
    event EventHandler<ClientConnectionInfo>? ClientConnectionInfoChanged;
    event EventHandler<List<VariableInfo>>? VariablesChanged;
    event EventHandler<List<VariableValueInfo>>? VariableValuesChanged;
    event EventHandler<LogEntry>? LogEntryAdded;
    event EventHandler<AutomationStateInfo>? AutomationStateInfoChanged;
    event EventHandler<AutomationTriggered>? AutomationTriggered;
    event EventHandler<Library>? LibraryChanged;
    event EventHandler<AIProvider>? AIProviderChanged;
    event EventHandler<AIConversation>? AIConversationChanged;

    void Handle(ClientHandlerInfo clientHandler);
    void Handle(AutomationHandlerInfo automationHandler);
    void Handle(ClientConnectionInfo clientConnectionInfo);
    void Handle(List<VariableInfo> variables);
    void Handle(List<VariableValueInfo> variableValues);
    void Handle(LogEntry logEntry);
    void Handle(AutomationStateInfo automationStateInfo);
    void Handle(AutomationTriggered automationTriggered);
    void Handle(Library library);
    void Handle(AIProvider aIProvider);
    void Handle(AIConversation aIConversation);
}
