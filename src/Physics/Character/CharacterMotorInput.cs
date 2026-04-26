using System.Numerics;

namespace Astroblock.Physics.Character;

/// <summary>
/// Frame input consumed by <see cref="ICharacterMotor"/>.
/// </summary>
public readonly record struct CharacterMotorInput(
    Vector2 MoveAxes,
    bool JumpPressed,
    Vector3 ThrusterAxes);
