namespace Astroblock.Gameplay.Interaction;

/// <summary>
/// Controls thruster unlock state and near-zero-g free-float mode activation.
/// </summary>
public sealed class ThrusterController
{
    private readonly float _zeroGravityEnterThreshold;
    private readonly float _zeroGravityExitThreshold;

    public ThrusterController(float zeroGravityThreshold, float exitHysteresisMultiplier = 1.25f)
    {
        if (zeroGravityThreshold <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(zeroGravityThreshold), zeroGravityThreshold, "Zero-g threshold must be positive.");
        }

        if (exitHysteresisMultiplier < 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(exitHysteresisMultiplier), exitHysteresisMultiplier, "Exit hysteresis multiplier must be at least 1.");
        }

        _zeroGravityEnterThreshold = zeroGravityThreshold;
        _zeroGravityExitThreshold = zeroGravityThreshold * exitHysteresisMultiplier;
    }

    public bool ThrustersUnlocked { get; private set; }

    public bool ThrusterModeEnabled { get; private set; }

    public void UnlockThrusters() => ThrustersUnlocked = true;

    public bool IsNearZeroGravity(float gravityMagnitude) => gravityMagnitude <= _zeroGravityEnterThreshold;

    public void Update(bool toggleRequested, float gravityMagnitude)
    {
        if (!ThrustersUnlocked)
        {
            ThrusterModeEnabled = false;
            return;
        }

        if (ThrusterModeEnabled && gravityMagnitude > _zeroGravityExitThreshold)
        {
            ThrusterModeEnabled = false;
            return;
        }

        if (!toggleRequested)
        {
            return;
        }

        if (ThrusterModeEnabled)
        {
            ThrusterModeEnabled = false;
            return;
        }

        if (gravityMagnitude <= _zeroGravityEnterThreshold)
        {
            ThrusterModeEnabled = true;
        }
    }
}
