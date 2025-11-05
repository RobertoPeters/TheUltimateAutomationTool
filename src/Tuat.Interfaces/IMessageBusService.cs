namespace Tuat.Interfaces;

public interface IMessageBusService
{
    ValueTask PublishAsync<T>(T message);
}
