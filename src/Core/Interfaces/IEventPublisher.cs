using Astroblock.Core.Events;

namespace Astroblock.Core.Interfaces;

/// <summary>
/// Minimal abstraction for publishing cross-module domain events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event to all current subscribers.
    /// </summary>
    /// <typeparam name="TEvent">Event payload type.</typeparam>
    /// <param name="domainEvent">The event payload to route.</param>
    void Publish<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent;
}
