using Tuat.Interfaces;
using Tuat.Models;

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

}

