using System.Numerics;
using Astroblock.Gravity.Queries;
using Astroblock.Physics.Character;

namespace Astroblock.IntegrationTests.Physics;

public sealed class GravityFieldAndMotorIntegrationTests
{
    [Fact]
    public void PlanetaryGravity_SamplesRemainContinuousAlongRadialPath()
    {
        var solver = new SimplePlanetaryGravityFieldSolver(
            sourcePosition: Vector3.Zero,
            gravitationalParameter: 1600f,
            minimumDistance: 5f);

        var previous = solver.SampleGravity(new Vector3(0f, 100f, 0f));

        for (var altitude = 99f; altitude >= 20f; altitude -= 1f)
        {
            var current = solver.SampleGravity(new Vector3(0f, altitude, 0f));

            Assert.True(float.IsFinite(current.Length()), $"Gravity magnitude must stay finite at altitude {altitude}.");
            Assert.True(Vector3.Dot(Vector3.Normalize(previous), Vector3.Normalize(current)) > 0.999f,
                $"Gravity direction should not flip at altitude {altitude}.");
            Assert.True(MathF.Abs(current.Length() - previous.Length()) < 0.5f,
                $"Adjacent gravity samples changed too abruptly at altitude {altitude}.");

            previous = current;
        }
    }

    [Fact]
    public void CharacterMotor_OrientationTracksChangingGravityWithoutJitter()
    {
        var motor = new SimpleCharacterMotor(
            initialState: new CharacterMotorState(
                Position: Vector3.Zero,
                Velocity: Vector3.Zero,
                Up: Vector3.UnitY,
                IsGrounded: true,
                ThrusterModeEnabled: false),
            orientationSharpness: 8f,
            zeroGravityThreshold: 0.02f);

        var lastUp = motor.State.Up;
        var gravityBase = new Vector3(0f, -9.81f, 0f);

        for (var i = 0; i < 120; i++)
        {
            var wobble = 0.1f * MathF.Sin(i * 0.15f);
            var gravity = Vector3.Normalize(new Vector3(wobble, -1f, 0.08f * wobble)) * gravityBase.Length();

            motor.Simulate(1f / 60f, new CharacterMotorInput(Vector2.Zero, JumpPressed: false, ThrusterAxes: Vector3.Zero), gravity);

            var up = motor.State.Up;
            Assert.True(MathF.Abs(up.Length() - 1f) < 0.0001f, "Up vector must remain normalized.");
            Assert.True(Vector3.Dot(up, -Vector3.Normalize(gravity)) > 0.985f, "Up vector should closely track negative gravity direction.");
            Assert.True(Vector3.Dot(up, lastUp) > 0.985f, "Up vector changed too abruptly between simulation steps.");

            lastUp = up;
        }
    }
}
