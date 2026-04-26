using Astroblock.Core.Interfaces;

namespace Astroblock.Core.Events;

/// <summary>
/// Thread-safe in-process event bus for domain event fan-out.
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly object _gate = new();
    private readonly Dictionary<Type, List<Delegate>> _subscribers = [];

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);

        lock (_gate)
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers = [];
                _subscribers[eventType] = handlers;
            }

            handlers.Add(handler);
        }

        return new Unsubscriber(() => Unsubscribe(eventType, handler));
    }

    public void Publish<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        Delegate[] snapshot;

        lock (_gate)
        {
            if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers) || handlers.Count == 0)
            {
                return;
            }

            snapshot = handlers.ToArray();
        }

        foreach (var handler in snapshot)
        {
            ((Action<TEvent>)handler)(domainEvent);
        }
    }

    private void Unsubscribe(Type eventType, Delegate handler)
    {
        lock (_gate)
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers))
            {
                return;
            }

            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _subscribers.Remove(eventType);
            }
        }
    }

    private sealed class Unsubscriber : IDisposable
    {
        private Action? _unsubscribe;

        public Unsubscriber(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
        }
    }
}
