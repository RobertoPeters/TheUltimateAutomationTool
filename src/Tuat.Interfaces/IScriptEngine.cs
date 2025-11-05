namespace Tuat.Interfaces;

public interface IScriptEngine : IDisposable
{
    public class FunctionParameter
    {
        public string Name { get; set; } = null!;
        public Type Type { get; set; } = null!;
    }

    void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler);
    void CallVoidFunction(string functionName, List<FunctionParameter>? functionParameters = null);
    void Execute(string script);
    object? Evaluate(string script);
    string GetDeclareFunction(string functionName, bool hasReturnValue, Type? returnValueType = null, List<FunctionParameter>? functionParameters = null);
}
