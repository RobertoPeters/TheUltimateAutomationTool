using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.ScriptEngineCSharp;

[DisplayName("CSharp")]
[Editor("Tuat.ScriptEngineCSharp.Editor", typeof(Editor))]
public class CSharpEngine : IScriptEngine
{
    DynamicScriptApi scriptApi = new();
    ScriptState<object>? scriptState;
    Dictionary<string, (IScriptEngine.FunctionReturnValue? returnValue, List<IScriptEngine.FunctionParameter>? functionParameters)> scriptMethodProtoTypes = new();

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript, List<AutomationInputVariable>? InputValues = null)
    {
        additionalScript = $"{additionalScript}\r\n{GetUserMethodsMapping()}";
        var systemMethods = new SystemMethods(clientService, dataService, variableService, automationHandler);
        scriptApi._systemMethods = systemMethods;
        var systemScript = GetSystemScript(clientService, instanceId, additionalScript, subAutomationParameters: automationHandler.Automation.SubAutomationParameters, inputValues: InputValues);
        var options = ScriptOptions.Default
            .AddReferences(typeof(DynamicScriptApi).Assembly)
            .AddImports("System", "System.Collections.Generic", "Tuat.Interfaces");

        scriptState = CSharpScript.RunAsync(systemScript, options, globals: scriptApi).Result;
    }

    public void HandleSubAutomationOutputVariables(List<AutomationOutputVariable> outputVariables)
    {
        foreach (var variable in outputVariables)
        {
            var scriptVariable = scriptState!.GetVariable(variable.Name);
            if (scriptVariable != null)
            {
                scriptVariable.Value = Convert.ChangeType(variable.Value, scriptVariable.Value.GetType());
            }
            else
            {
                string valueText = "";
                if (variable.Value == null)
                {
                    valueText = "null";
                }
                else if (variable.Value is string)
                {
                    valueText = $"\"{variable.Value}\"";
                }
                else
                {
                    valueText = variable.Value.ToString()!.Replace(",", ".");
                }

                Execute($"var {variable.Name} = {valueText};");
            }
        }
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
        return scriptState?.Variables.Select(x => new IScriptEngine.ScriptVariable(x.Name, x.Value)).ToList() ?? [];
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
        scriptState = scriptState!.ContinueWithAsync(script).Result;
    }

    public object? Evaluate(string script)
    {
        scriptState = scriptState!.ContinueWithAsync(script).Result;
        return scriptState.ReturnValue;
    }

    public void Dispose()
    {
    }

    public string GetDeclareFunction(string functionName, IScriptEngine.FunctionReturnValue? returnValue = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
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

    public string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null, List<SubAutomationParameter>? subAutomationParameters = null, List<AutomationInputVariable>? inputValues = null)
    {
        var script = new StringBuilder();
        if (!string.IsNullOrEmpty(additionalScript))
        {
            script.AppendLine(additionalScript);
        }
        script.AppendLine();

        script.AppendLine(SystemMethods.SystemScript());
        script.AppendLine();
        script.AppendLine($""""var instanceId = "{(instanceId ?? Guid.Empty).ToString()}"; """");
        script.AppendLine();
 
        var clients = clientService.GetClients();
        List<string> clientTypesHandled = [];
        foreach (var client in clients)
        {
            if (clientTypesHandled.Contains(client.Client.ClientType))
            {
                continue;
            }

            var createVariableMethods = client.CreateVariableOnClientMethods();
            var createExecuteMethods = client.CreateExecuteOnClientMethods();
            if (createVariableMethods.Any() || createExecuteMethods.Any())
            {
                script.AppendLine("//===================================================");
                script.AppendLine($"// client helper methods for {client.Client.Name}");
                script.AppendLine("//===================================================");

                foreach (var method in createVariableMethods)
                {
                    script.AppendLine($"""//{method.description}""");
                    script.AppendLine($"""//{method.example}""");
                    script.AppendLine($"""int {method.methodName}(string name, object? data = null, object[]? mockingOptions = null, int clientId = {client.Client.Id})""");
                    script.AppendLine("{");
                    script.AppendLine($"   return CreateVariableOnClient(name, clientId, {(method.isAutomationVariable ? "true" : "false")},  {(method.persistant ? "true" : "false")}, data, mockingOptions);");
                    script.AppendLine("}");
                }

                foreach (var method in createExecuteMethods)
                {
                    script.AppendLine($"""//{method.description}""");
                    script.AppendLine($"""//{method.example}""");
                    script.AppendLine($"""bool {method.methodName}(int? variableId = null, object? parameter1 = null, object? parameter2 = null, object? parameter3 = null, int clientId = {client.Client.Id})""");
                    script.AppendLine("{");
                    script.AppendLine($"""   return ExecuteOnClient(clientId, variableId, "{method.command}", parameter1, parameter2, parameter3);""");
                    script.AppendLine("}");
                }
            }
        }

        script.AppendLine();
        subAutomationParameters?.ForEach(p =>
        {
            var inputVar = inputValues?.FirstOrDefault(x => string.Compare(x.Name, p.Name, true) == 0);
            if (inputVar != null)
            {
                if (inputVar.Value == null)
                {
                    script.AppendLine($"var {p.ScriptVariableName} = null;");
                }
                else if (inputVar.Value is string v)
                {
                    script.AppendLine($"""var {p.ScriptVariableName} = "{inputVar.Value}"; """);
                }
                else
                {
                    script.AppendLine($"var {p.ScriptVariableName} = {inputVar.Value!.ToString()?.Replace(",", ".")};");
                }
            }
            else
            {
                script.AppendLine($"var {p.ScriptVariableName} = {p.DefaultValue};");
            }
        });

        return script.ToString();
    }
}
