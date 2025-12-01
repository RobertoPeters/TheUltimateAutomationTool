using System.Collections.Concurrent;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.Automations;

public class AutomationService(IDataService _dataService, IClientService _clientService, IVariableService _variableService, IMessageBusService _messageBusService) : IAutomationService
{
    private readonly ConcurrentDictionary<int, IAutomationHandler> _handlers = [];
    private Timer? _slowTriggerTimer;

    public Task StartAsync()
    {
        var automations = _dataService.GetAutomations();
        foreach (var automation in automations)
        {
            AddAutomation(automation);
        }
        _slowTriggerTimer = new Timer((state) =>
        {
            foreach (var item in _handlers)
            {
                try
                {
                    item.Value.RequestTriggerProcess();
                }
                catch
                {
                    //nothing
                }
            }
            _slowTriggerTimer?.Change(3000, Timeout.Infinite);
        }, null, 5000, Timeout.Infinite);

        return Task.CompletedTask;
    }

    public List<IAutomationHandler> GetAutomations()
    {
        return _handlers.Values.ToList();
    }

    public IAutomationHandler GetAutomation(int id)
    {
        return _handlers[id];
    }

    public async Task Handle(List<VariableInfo> variableInfos)
    {
        foreach (var automationHandler in _handlers.Values.ToList())
        {
            await automationHandler.Handle(variableInfos);
        }
    }

    public async Task Handle(List<VariableValueInfo> variableValueInfos)
    {
        foreach (var automationHandler in _handlers.Values.ToList())
        {
            await automationHandler.Handle(variableValueInfos);
        }
    }

    public async Task Handle(Automation automation)
    {
        IAutomationHandler? automationHandler = null;
        if (automation.Id < 0)
        {
            automationHandler = RemoveAutomationHandler(-automation.Id);
            if (automationHandler != null)
            {
                automationHandler.Automation.Id = automation.Id;
            }
        }
        else if (_handlers.TryGetValue(automation.Id, out automationHandler))
        {
            await automationHandler.UpdateAsync(automation);
        }
        else
        {
            automationHandler = AddAutomation(automation);
        }

        if (automationHandler != null)
        {
            await _messageBusService.PublishAsync(new AutomationHandlerInfo(automationHandler!));
        }
    }

    private IAutomationHandler? RemoveAutomationHandler(int id)
    {
        IAutomationHandler? automationHandler = null;
        if (_handlers.TryRemove(id, out automationHandler))
        {
            automationHandler.Dispose();
        }
        return automationHandler;
    }

    private IAutomationHandler? AddAutomation(Automation automation)
    {
        IAutomationHandler? automationHandler = null;

        var type = Tuat.Helpers.Generics.Generic.ComponentType(automation.AutomationType)!;
        automationHandler = (IAutomationHandler?)Activator.CreateInstance(type, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new object[] { automation, _clientService, _dataService, _variableService, _messageBusService }, null);

        if (automationHandler != null)
        {
            if (!_handlers.TryAdd(automation.Id, automationHandler))
            {
                automationHandler = null;
            }
            if (automationHandler != null && !automationHandler.Automation.IsSubAutomation)
            {
                automationHandler.Start();
            }
        }
        return automationHandler;
    }
}

