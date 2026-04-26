using System.Numerics;

namespace Astroblock.Physics.Character;

/// <summary>
/// Mutable character simulation state.
/// </summary>
public readonly record struct CharacterMotorState(
    Vector3 Position,
    Vector3 Velocity,
    Vector3 Up,
    bool IsGrounded,
    bool ThrusterModeEnabled);
