using Radzen;
using Tuat.Components;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

var settingsFolder = Path.Combine(builder.Environment.ContentRootPath, "Settings");
var settingsPath = Path.Combine(settingsFolder, "appsettings.json");

if (!Directory.Exists(settingsFolder))
{
    Directory.CreateDirectory(settingsFolder);
}
if (!File.Exists(settingsPath))
{
    File.Copy(Path.Combine(builder.Environment.ContentRootPath, "appsettings.json"), settingsPath);
}

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(settingsPath, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Host.UseWolverine();

builder.Services.AddSingleton<Tuat.Interfaces.IRepository, Tuat.Repository.DataRepository>();
builder.Services.AddSingleton<Tuat.Interfaces.IMessageBusService, Tuat.MessageBus.MessageBusService>();
builder.Services.AddSingleton<Tuat.Interfaces.IDataService, Tuat.Data.DataService>();
builder.Services.AddSingleton<Tuat.Interfaces.IVariableService, Tuat.Variables.VariableService>();
builder.Services.AddSingleton<Tuat.Interfaces.IClientService, Tuat.Clients.ClientService>();
builder.Services.AddSingleton<Tuat.Interfaces.IAutomationService, Tuat.Automations.AutomationService>();
builder.Services.AddSingleton<Tuat.Interfaces.IUIEventRegistration, Tuat.Components.UIEventRegistration>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<TooltipService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

bool _pipelineReady = false;
app.Use(async (context, next) =>
{
    if (!_pipelineReady)
    {
        await Task.Run(() =>
        {
            while (!_pipelineReady)
            {
                Thread.Sleep(500);
            }
        });
    }
    await next.Invoke();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var t = new Thread(new ThreadStart(
    async () =>
    {
        await Task.Delay(2000);
        var dataService = app.Services.GetRequiredService<Tuat.Interfaces.IDataService>();
        await dataService.StartAsync();
        var variableService = app.Services.GetRequiredService<Tuat.Interfaces.IVariableService>();
        await variableService.StartAsync();
        var clientService = app.Services.GetRequiredService<Tuat.Interfaces.IClientService>();
        await clientService.StartAsync();
        var automationService = app.Services.GetRequiredService<Tuat.Interfaces.IAutomationService>();
        await automationService.StartAsync();

        var clients = dataService.GetClients();

        if (!clients.Any(x => x.Name == "Generic"))
        {
            var genericClient = new Tuat.Models.Client()
            {
                Id = 0,
                Name = "Generic",
                Enabled = true,
                ClientType = typeof(Tuat.GenericClient.GenericClientHandler).FullName!,
                Settings = ""
            };
            await dataService.AddOrUpdateClientAsync(genericClient);
        }

        //var timerClient = new Tuat.Models.Client()
        //{
        //    Id = 2,
        //    Name = "Timer",
        //    Enabled = true,
        //    ClientType = ClientType.Timer,
        //    Data = ""
        //};


        _pipelineReady = true;
    }));
t.Start();


await app.RunAsync();
