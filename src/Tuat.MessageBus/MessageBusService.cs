using Microsoft.Extensions.DependencyInjection;
using Tuat.Interfaces;
using Wolverine;

namespace Tuat.MessageBus;

public class MessageBusService(IServiceScopeFactory _serviceScopeFactory): IMessageBusService
{
    public ValueTask PublishAsync<T>(T message)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        return bus.PublishAsync(message, null);
    }

}
