using Tuat.Models;

namespace Tuat.Interfaces;

public interface IScriptEngine : IDisposable
{
    public class FunctionParameter
    {
        public string Name { get; set; } = null!;
        public Type Type { get; set; } = null!;
        public bool Nullable { get; set; }
        public object? Value { get; set; }
    }

    public class FunctionReturnValue
    {
        public Type Type { get; set; } = null!;
        public bool Nullable { get; set; }
    }

    public record ScriptVariable(string Name, object? Value);

    string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null, List<SubAutomationParameter>? subAutomationParameters = null, List<AutomationInputVariable>? inputValues = null);
    void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript, List<AutomationInputVariable>? InputValues = null);
    void CallVoidFunction(string functionName, List<FunctionParameter>? functionParameters = null);
    T CallFunction<T>(string functionName, List<FunctionParameter>? functionParameters = null);
    string GetReturnTrueStatement();
    void Execute(string script);
    object? Evaluate(string script);
    string GetDeclareFunction(string functionName, FunctionReturnValue? returnValue = null, List<FunctionParameter>? functionParameters = null, string? body = null);
    List<ScriptVariable> GetScriptVariables();
    void HandleSubAutomationOutputVariables(List<AutomationOutputVariable> outputVariables);
}
