using Astroblock.Gameplay.Interaction;

namespace Astroblock.IntegrationTests.Gameplay;

public sealed class ThrusterControllerIntegrationTests
{
    [Fact]
    public void ThrusterController_TransitionsRespectUnlockAndZeroGThreshold()
    {
        var controller = new ThrusterController(zeroGravityThreshold: 0.2f);

        controller.Update(toggleRequested: true, gravityMagnitude: 0.1f);
        Assert.False(controller.ThrusterModeEnabled);

        controller.UnlockThrusters();
        controller.Update(toggleRequested: true, gravityMagnitude: 0.3f);
        Assert.False(controller.ThrusterModeEnabled);

        controller.Update(toggleRequested: true, gravityMagnitude: 0.15f);
        Assert.True(controller.ThrusterModeEnabled);

        controller.Update(toggleRequested: false, gravityMagnitude: 0.22f);
        Assert.True(controller.ThrusterModeEnabled);

        controller.Update(toggleRequested: false, gravityMagnitude: 0.3f);
        Assert.False(controller.ThrusterModeEnabled);
    }
}
