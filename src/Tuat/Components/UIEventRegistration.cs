using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.Components;

public class UIEventRegistration: IUIEventRegistration
{
    public event EventHandler<ClientHandlerInfo>? ClientHandlerChanged;
    public event EventHandler<AutomationHandlerInfo>? AutomationHandlerChanged;
    public event EventHandler<ClientConnectionInfo>? ClientConnectionInfoChanged;

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

}
