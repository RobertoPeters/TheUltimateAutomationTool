using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.Components;

public class UIEventRegistration: IUIEventRegistration
{
    public event EventHandler<ClientHandlerInfo>? ClientHandlerChanged;
    public event EventHandler<AutomationHandlerInfo>? AutomationHandlerChanged;
    public event EventHandler<ClientConnectionInfo>? ClientConnectionInfoChanged;
    public event EventHandler<List<VariableInfo>>? VariablesChanged;
    public event EventHandler<List<VariableValueInfo>>? VariableValuesChanged;
    public event EventHandler<LogEntry>? LogEntryAdded;
    public event EventHandler<AutomationStateInfo>? AutomationStateInfoChanged;
    public event EventHandler<AutomationTriggered>? AutomationTriggered;
    public event EventHandler<Library>? LibraryChanged;
    public event EventHandler<AIProvider>? AIProviderChanged;

    public void Handle(ClientHandlerInfo clientHandler)
    {
        ClientHandlerChanged?.Invoke(this, clientHandler);
    }

    public void Handle(AutomationHandlerInfo automationHandler)
    {
        AutomationHandlerChanged?.Invoke(this, automationHandler);
    }

    public void Handle(ClientConnectionInfo clientConnectionInfo)
    {
        ClientConnectionInfoChanged?.Invoke(this, clientConnectionInfo);
    }

    public void Handle(List<VariableInfo> variables)
    {
        VariablesChanged?.Invoke(this, variables);

    }

    public void Handle(List<VariableValueInfo> variableValues)
    {
        VariableValuesChanged?.Invoke(this, variableValues);
    }

    public void Handle(LogEntry logEntry)
    {
        LogEntryAdded?.Invoke(this, logEntry);
    }

    public void Handle(AutomationStateInfo automationStateInfo)
    {
        AutomationStateInfoChanged?.Invoke(this, automationStateInfo);
    }

    public void Handle(AutomationTriggered automationTriggered)
    {
        AutomationTriggered?.Invoke(this, automationTriggered);
    }

    public void Handle(Library library)
    { 
        LibraryChanged?.Invoke(this, library);
    }

    public void Handle(AIProvider aIProvider)
    {
        AIProviderChanged?.Invoke(this, aIProvider);
    }
}
