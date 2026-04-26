namespace Astroblock.Core.Interfaces;

/// <summary>
/// Convenience contract for implementations that provide both publish and subscribe behaviors.
/// </summary>
public interface IEventBus : IEventPublisher, IEventSubscriber
{
}
