using Tuat.Interfaces;

namespace Tuat.MessageHandlers;

public static class UIEventRegistrationMessageHandler
{
    public static void Handle(ClientHandlerInfo clientHandler, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(clientHandler);
    }

    public static void Handle(AutomationHandlerInfo automationHandler, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(automationHandler);
    }

    public static void Handle(ClientConnectionInfo clientConnectionInfo, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(clientConnectionInfo);
    }

    public static void Handle(List<VariableInfo> variableInfo, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(variableInfo);
    }

    public static void Handle(List<VariableValueInfo> variableValueInfo, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(variableValueInfo);
    }

    public static void Handle(LogEntry logEntry, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(logEntry);
    }
    public static void Handle(AutomationStateInfo automationStateInfo, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(automationStateInfo);
    }
    public static void Handle(AutomationTriggered automationTriggered, IUIEventRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(automationTriggered);
    }
}

