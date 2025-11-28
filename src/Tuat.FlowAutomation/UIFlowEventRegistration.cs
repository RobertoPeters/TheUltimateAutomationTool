namespace Tuat.FlowAutomation;

public class UIFlowEventHandler
{
    public event EventHandler<PayloadInfo>? PayloadsUpdated;

    public void Handle(PayloadInfo payloadInfo)
    {
        PayloadsUpdated?.Invoke(this, payloadInfo);
    }
}

public static class UIFlowEventRegistration
{
    public static void Handle(PayloadInfo payloadInfo, UIFlowEventHandler uiEventRegistration)
    {
        uiEventRegistration.Handle(payloadInfo);
    }
}
