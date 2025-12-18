using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.LibraryEditor.Sandbox;

public class LibraryTester(IDataService _realDataService) : IAsyncDisposable
{
    private IAutomationHandler? _automationHandler;
    private IMessageBusService? _messageBusService;
    private IClientService? _clientService;
    private IDataService? _dataService;
    private IVariableService? _variableService;
    private IUIEventRegistration? _uiEventRegistration;

    public IAutomationHandler? AutomationHandler => _automationHandler;
    public IDataService? DataService => _dataService;
    public IVariableService? VariableService => _variableService;
    public IUIEventRegistration? UIEventRegistration => _uiEventRegistration;
    public IClientService? ClientService => _clientService;
    public Guid Id { get; } = Guid.NewGuid();

    public async Task StartAsync(Library library)
    {
        _messageBusService = new Sandbox.MessageBusService(this);
        _dataService = new Sandbox.DataService(_realDataService);
        _variableService = new Tuat.Variables.VariableService(_dataService, _messageBusService);
        _clientService = new Tuat.Clients.ClientService(_dataService, _variableService, _messageBusService);
        _uiEventRegistration = new UIEventRegistration();

        await _dataService.StartAsync();
        await _variableService.StartAsync();
        await _clientService.StartAsync();

        var automation = new Automation
        {
            Id = 0,
            Name = "Sandbox Automation",
            AutomationType = typeof(AutomationHandler).FullName!,
            Data = "",
            ScriptType = library.ScriptType,
            IncludeScriptId = library.Id,
            Enabled = true
        };
        await _dataService.AddOrUpdateAutomationAsync(automation);

        _automationHandler = new AutomationHandler(
            automation,
            _clientService,
            _dataService,
            _variableService,
            _messageBusService
        );

        _automationHandler.Start();
    }

    public async Task StopAsync()
    {
        _automationHandler?.Dispose();
        _automationHandler = null;
        if (_clientService != null)
        {
            await _clientService.DisposeAsync();
            _clientService = null;
        }
        _variableService?.Dispose();
        _variableService = null;
        _messageBusService = null;
        _dataService = null;
        _uiEventRegistration = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
