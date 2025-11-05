using Tuat.Models;

namespace Tuat.Interfaces;

public interface IUIEventRegistration
{
    event EventHandler<ClientHandlerInfo>? ClientHandlerChanged;
    event EventHandler<AutomationHandlerInfo>? AutomationHandlerChanged;
    event EventHandler<ClientConnectionInfo>? ClientConnectionInfoChanged;
    //public event EventHandler<LogEntry>? LogEntryAdded;
    //public event EventHandler<StateMachineHandler.StateMachineHandlerInfo>? StateMachineHandlerInfoChanged;
    //public event EventHandler<FlowHandler.FlowHandlerInfo>? FlowHandlerInfoChanged;
    //public event EventHandler<ScriptHandler.ScriptHandlerInfo>? ScriptHandlerInfoChanged;
    //public event EventHandler<List<VariableService.VariableInfo>>? VariablesChanged;
    //public event EventHandler<List<VariableService.VariableValueInfo>>? VariableValuesChanged;
    //public event EventHandler<ClientConnectionInfo>? ClientConnectionInfoChanged;
    //public event EventHandler<AutomationInfo>? AutomationInfoChanged;

    void Handle(ClientHandlerInfo clientHandler);
    void Handle(AutomationHandlerInfo automationHandler);
     void Handle(ClientConnectionInfo clientConnectionInfo);
    //public void Handle(LogEntry logEntry)
    //{
    //    LogEntryAdded?.Invoke(this, logEntry);
    //}

    //public void Handle(StateMachineHandler.StateMachineHandlerInfo stateMachineHandlerInfo)
    //{
    //    StateMachineHandlerInfoChanged?.Invoke(this, stateMachineHandlerInfo);
    //}

    //public void Handle(FlowHandler.FlowHandlerInfo flowHandlerInfo)
    //{
    //    FlowHandlerInfoChanged?.Invoke(this, flowHandlerInfo);
    //}

    //public void Handle(ScriptHandler.ScriptHandlerInfo scriptHandlerInfo)
    //{
    //    ScriptHandlerInfoChanged?.Invoke(this, scriptHandlerInfo);
    //}

    //public void Handle(List<VariableService.VariableInfo> variables)
    //{
    //    VariablesChanged?.Invoke(this, variables);
    //}

    //public void Handle(List<VariableService.VariableValueInfo> variableValues)
    //{
    //    VariableValuesChanged?.Invoke(this, variableValues);
    //}

   

    //public void Handle(AutomationInfo automationInfo)
    //{
    //    AutomationInfoChanged?.Invoke(this, automationInfo);
    //}
}
