using System.ComponentModel;
using System.Text;
using Tuat.AutomationHelper;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.StateMachineAutomation;

[DisplayName("State Machine")]
[Editor("Tuat.StateMachineAutomation.AutomationSettings", typeof(AutomationSettings))]
[Editor("Tuat.StateMachineAutomation.Editor", typeof(Editor))]
public class StateMachineHandler : BaseAutomationHandler<AutomationProperties>, IAutomationHandler
{
    public State? CurrentState { get; set; }

    private List<DateTime> _scheduledTimes = [];

    public StateMachineHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
        : base(automation, clientService, dataService, variableService, messageBusService)
    {
    }

    protected override void RunProcess(IScriptEngine scriptEngine)
    {
        //check cooling down
        //if there is a loop with pass through transitions, it can cause high CPU usage
        //for now: maximum of 10 runs in 2 seconds
        var scheduleWindow = DateTime.UtcNow.AddSeconds(-2);
        _scheduledTimes = _scheduledTimes.Where(t => t > scheduleWindow).ToList();
        if (_scheduledTimes.Count > 10)
        {
            return;
        }

        _scheduledTimes.Add(DateTime.UtcNow);
        if (CurrentState == null)
        {
            ChangeState(scriptEngine, GetStartState());
            return;
        }
        var transitions = _automationProperties.Transitions.Where(t => t.FromStateId == CurrentState.Id).ToList();
        if (transitions.Count == 0)
        {
            RunningState = AutomationRunningState.Finished;
            return;
        }
        foreach(var transition in transitions)
        {
            var result = scriptEngine.CallFunction<bool>(GetScriptTransitionMethodName(transition));
            if (result)
            {
                var nextState = _automationProperties.States.First(s => s.Id == transition.ToStateId);
                ChangeState(scriptEngine, nextState);
                return;
            }
        }
    }

    protected override void RunStart(IScriptEngine scriptEngine, Guid instanceId, List<AutomationInputVariable>? InputValues = null, int? topAutomationId = null)
    {
        CurrentState = null;
        _scheduledTimes.Clear();
        var startState = GetStartState();

        var script = new StringBuilder();

        foreach(var state in _automationProperties.States)
        {
            script.AppendLine(scriptEngine.GetDeclareFunction(GetScriptActionMethodName(state), body: state.EntryAction));
        }

        foreach (var transition in _automationProperties.Transitions)
        {
            var body = string.IsNullOrWhiteSpace(transition.Condition) ? scriptEngine.GetReturnTrueStatement() : transition.Condition;
            script.AppendLine(scriptEngine.GetDeclareFunction(GetScriptTransitionMethodName(transition), returnValue: new IScriptEngine.FunctionReturnValue() { Nullable=false, Type=typeof(bool) }, body: body));
        }

        if (!string.IsNullOrWhiteSpace(_automationProperties.PreStartAction))
        {
            script.AppendLine(_automationProperties.PreStartAction);
        }

        scriptEngine.Initialize(_clientService, _dataService, _variableService, this, instanceId, script.ToString(), InputValues, topAutomationId);
    }

    private void ChangeState(IScriptEngine scriptEngine, State state)
    {
        if (state != CurrentState)
        {
            CurrentState = state;
            AddLogAsync($"Changed to state: {state.Name}");
            scriptEngine.CallVoidFunction(GetScriptActionMethodName(state));
            if (state.IsSubState)
            {
                StartSubAutomation(scriptEngine, state);
            }
            TriggerProcess();
        }
    }

    private void StartSubAutomation(IScriptEngine scriptEngine, State state)
    {
        List<AutomationInputVariable> inputVariables = [];
        if (state.SubStateParameters?.Any() == true)
        {
            var scriptVariables = scriptEngine.GetScriptVariables();
            var subAutomationParameters = _dataService.GetAutomations()
                    .First(x => x.Id == state.SubStateMachineId).SubAutomationParameters
                    .Where(x => x.IsInput)
                    .ToList();
            foreach (var subStateVariable in state.SubStateParameters)
            {
                var subAutomationParameter = subAutomationParameters.FirstOrDefault(x => x.Id == subStateVariable.Id);
                var scriptVariable = scriptVariables.FirstOrDefault(x => x.Name == subStateVariable.ScriptVariableName);
                if (subAutomationParameter != null)
                {
                    inputVariables.Add(new AutomationInputVariable() { Name = subAutomationParameter.Name, Value = scriptVariable?.Value });
                }
            }
        }
        StartSubAutomation(state.SubStateMachineId!.Value, inputVariables);
    }

    private State GetStartState()
    {
        var startStates = _automationProperties.States.Where(s => s.IsStartState).ToList();
        if (startStates.Count == 0)
        {
            throw new InvalidOperationException("Start state not found.");
        }
        else if (startStates.Count > 1)
        {
            throw new InvalidOperationException("Multiple start states found.");
        }
        return startStates[0];
    }

    private string GetScriptActionMethodName(State state)
    {
        return $"state_action_{state.Id.ToString("N")}";
    }

    private string GetScriptTransitionMethodName(Transition transition)
    {
        return $"transition_{transition.Id.ToString("N")}";
    }
}
