using Tuat.Models;

namespace Tuat.Interfaces;

public interface IAutomationService
{
    Task StartAsync();
    List<IAutomationHandler> GetAutomations();
    IAutomationHandler GetAutomation(int id);
    Task Handle(List<VariableInfo> variableInfos);
    Task Handle(List<VariableValueInfo> variableValueInfos);
    Task Handle(Automation automation);
}
