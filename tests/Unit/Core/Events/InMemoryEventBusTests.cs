using Astroblock.Core.Coords;
using Astroblock.Core.Events;

namespace Astroblock.UnitTests.Core.Events;

public sealed class InMemoryEventBusTests
{
    [Fact]
    public void Publish_HandlerSubscribedForEvent_ReceivesEvent()
    {
        var eventBus = new InMemoryEventBus();
        var expected = new ChunkLoaded(ChunkCoord3.FromLongs(1, 2, 3));
        ChunkLoaded? received = null;

        using var _ = eventBus.Subscribe<ChunkLoaded>(evt => received = evt);

        eventBus.Publish(expected);

        Assert.Equal(expected, received);
    }

    [Fact]
    public void Publish_MultipleHandlers_InvokesInSubscriptionOrder()
    {
        var eventBus = new InMemoryEventBus();
        var calls = new List<string>();

        using var first = eventBus.Subscribe<ChunkLoaded>(_ => calls.Add("first"));
        using var second = eventBus.Subscribe<ChunkLoaded>(_ => calls.Add("second"));
        using var third = eventBus.Subscribe<ChunkLoaded>(_ => calls.Add("third"));

        eventBus.Publish(new ChunkLoaded(ChunkCoord3.FromLongs(0, 0, 0)));

        Assert.Equal(["first", "second", "third"], calls);
    }

    [Fact]
    public void DisposeSubscription_UnsubscribesHandler_FromFuturePublishes()
    {
        var eventBus = new InMemoryEventBus();
        var callCount = 0;
        var subscription = eventBus.Subscribe<ChunkLoaded>(_ => callCount++);

        eventBus.Publish(new ChunkLoaded(ChunkCoord3.FromLongs(0, 0, 0)));
        subscription.Dispose();
        eventBus.Publish(new ChunkLoaded(ChunkCoord3.FromLongs(0, 0, 0)));

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Publish_NoSubscribers_IsNoOp()
    {
        var eventBus = new InMemoryEventBus();

        var exception = Record.Exception(() =>
            eventBus.Publish(new ChunkLoaded(ChunkCoord3.FromLongs(9, 9, 9))));

        Assert.Null(exception);
    }
}
