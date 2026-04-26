using Astroblock.Core.Events;

namespace Astroblock.Core.Interfaces;

/// <summary>
/// Minimal abstraction for subscribing to cross-module domain events.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Registers a handler for a given event payload type.
    /// </summary>
    /// <typeparam name="TEvent">Event payload type.</typeparam>
    /// <param name="handler">Callback invoked when matching events are published.</param>
    /// <returns>A disposable token that removes the subscription when disposed.</returns>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : IDomainEvent;
}
