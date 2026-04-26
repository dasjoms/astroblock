using System.Numerics;

namespace Astroblock.Physics.Character;

/// <summary>
/// Simulates player-character movement for arbitrary up vectors.
/// </summary>
public interface ICharacterMotor
{
    CharacterMotorState State { get; }

    void SetGrounded(bool isGrounded);

    void SetThrusterMode(bool isEnabled);

    void Simulate(float deltaSeconds, in CharacterMotorInput input, in Vector3 gravity);
}
