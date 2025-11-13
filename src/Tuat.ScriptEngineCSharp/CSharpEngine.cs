using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Text;
using Tuat.Interfaces;
using static Tuat.Interfaces.IScriptEngine;

namespace Tuat.ScriptEngineCSharp;

[DisplayName("CSharp")]
[Editor("Tuat.ScriptEngineCSharp.Editor", typeof(Editor))]
public class CSharpEngine : IScriptEngine
{
    DynamicScriptApi scriptApi = new();
    Dictionary<string, (FunctionReturnValue? returnValue, List<IScriptEngine.FunctionParameter>? functionParameters)> scriptMethodProtoTypes = new();

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript)
    {
        additionalScript = $"{additionalScript}\r\n{GetUserMethodsMapping()}";
        var systemMethods = new SystemMethods(clientService, dataService, variableService, automationHandler);
        scriptApi._systemMethods = systemMethods;
        var systemScript = GetSystemScript(clientService, instanceId, additionalScript);
        try
        {
            var options = ScriptOptions.Default
                .AddReferences(typeof(DynamicScriptApi).Assembly)
                .AddImports("System", "System.Collections.Generic");

            var scriptState = CSharpScript.RunAsync(systemScript, options, globals: scriptApi).Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij compileren script: {ex.Message}");
            return;
        }
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        var methodProtoType = scriptMethodProtoTypes[functionName];
        var scriptMethod = scriptApi.Methods[functionName];
        if (methodProtoType.functionParameters?.Any() != true)
        {
            var action = (Action)scriptMethod;
            action();
        }
    }

    public void Execute(string script)
    {
        //_engine?.Execute(script);
    }

    public object? Evaluate(string script)
    {
        return null;
        //return _engine?.Evaluate(script)?.ToObject();
    }

    public void Dispose()
    {
    }

    public string GetDeclareFunction(string functionName, FunctionReturnValue? returnValue = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        scriptMethodProtoTypes.Add(functionName, (returnValue, functionParameters));

        var result = new StringBuilder();
        result.Append($"void {functionName}("); //todo return type
        //todo
        result.AppendLine(")");
        result.AppendLine("{");
        if (body != null)
        {
            result.AppendLine(body);
        }
        result.AppendLine("}");
        return result.ToString();
    }

    private string GetUserMethodsMapping()
    {
        var script = new StringBuilder();
        foreach (var methodProtoType in scriptMethodProtoTypes)
        {
            if (methodProtoType.Value.functionParameters?.Any() != true)
            {
                if (methodProtoType.Value.returnValue == null)
                {
                    script.AppendLine($"""Methods["{methodProtoType.Key}"] = (Action){methodProtoType.Key};""");
                }
            }
        }
        return script.ToString();
    }

    public string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null)
    {
        var script = new StringBuilder();
        script.AppendLine(SystemMethods.SystemScript());
        script.AppendLine();
        script.AppendLine($""""var instanceId = "{(instanceId ?? Guid.Empty).ToString()}"; """");
        script.AppendLine();
        if (!string.IsNullOrEmpty(additionalScript))
        {
            script.AppendLine(additionalScript);
        }

        return script.ToString();
    }
}
