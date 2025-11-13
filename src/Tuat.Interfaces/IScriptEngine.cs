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


    string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null);
    void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript);
    void CallVoidFunction(string functionName, List<FunctionParameter>? functionParameters = null);
    void Execute(string script);
    object? Evaluate(string script);
    string GetDeclareFunction(string functionName, FunctionReturnValue? returnValue = null, List<FunctionParameter>? functionParameters = null, string? body = null);
}
