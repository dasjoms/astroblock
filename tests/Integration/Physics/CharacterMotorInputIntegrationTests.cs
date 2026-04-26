using System.Numerics;
using Astroblock.Physics.Character;

namespace Astroblock.IntegrationTests.Physics;

public sealed class CharacterMotorInputIntegrationTests
{
    [Fact]
    public void MotorInput_WalkAxesIncreasePlanarVelocityUnderGravity()
    {
        var motor = new SimpleCharacterMotor(
            new CharacterMotorState(
                Position: Vector3.Zero,
                Velocity: Vector3.Zero,
                Up: Vector3.UnitY,
                IsGrounded: true,
                ThrusterModeEnabled: false));

        var gravity = new Vector3(0f, -9.81f, 0f);
        var input = new CharacterMotorInput(
            MoveAxes: new Vector2(1f, 0f),
            JumpPressed: false,
            ThrusterAxes: Vector3.Zero);

        motor.Simulate(1f / 60f, input, gravity);

        var planarSpeed = new Vector2(motor.State.Velocity.X, motor.State.Velocity.Z).Length();
        Assert.True(planarSpeed > 0.1f, "WASD movement should build lateral speed.");
        Assert.True(motor.State.Velocity.Y < 0f, "Gravity should continue to influence vertical velocity.");
    }

    [Fact]
    public void MotorInput_ThrusterAxesAccelerateInZeroGMode()
    {
        var motor = new SimpleCharacterMotor(
            new CharacterMotorState(
                Position: Vector3.Zero,
                Velocity: Vector3.Zero,
                Up: Vector3.UnitY,
                IsGrounded: false,
                ThrusterModeEnabled: true));

        var input = new CharacterMotorInput(
            MoveAxes: Vector2.Zero,
            JumpPressed: false,
            ThrusterAxes: new Vector3(0f, 0f, 1f));

        motor.Simulate(0.5f, input, gravity: Vector3.Zero);

        Assert.True(motor.State.Velocity.Z > 4.9f, "Thruster input should accelerate the motor in zero-g mode.");
        Assert.True(MathF.Abs(motor.State.Velocity.Y) < 0.0001f, "No gravity should avoid vertical drift in this scenario.");
    }
}
