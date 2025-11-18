using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.StateMachineAutomation;

public class StateMachineStateNodeModel : NodeModel
{
    public StateMachineStateNodeModel(State state, Automation automation, Type? scriptEditorType, IScriptEngine? scriptEngine, Point? position = null) : base(position) 
    {
        State = state;
        Automation = automation;
        ScriptEditorType = scriptEditorType;
        ScriptEngine = scriptEngine;
    }

    public State State { get; set; }
    public Automation Automation { get; set; }
    public Type? ScriptEditorType { get; set; }
    public IScriptEngine? ScriptEngine { get; set; }
}