using Wolverine.Attributes;
[assembly: WolverineModule]

namespace Tuat.FlowAutomation;

public class UIFlowEventHandlerRegistration
{
    public event EventHandler<PayloadInfo>? PayloadsUpdated;

    public void Handle(PayloadInfo payloadInfo)
    {
        PayloadsUpdated?.Invoke(this, payloadInfo);
    }
}

public static class UIFlowEventRegistrationMessageHandler
{
    public static void Handle(PayloadInfo payloadInfo, UIFlowEventHandlerRegistration uiEventRegistration)
    {
        uiEventRegistration.Handle(payloadInfo);
    }
}
