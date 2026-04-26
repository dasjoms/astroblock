using System.Numerics;

namespace Astroblock.Physics.Character;

/// <summary>
/// First-pass motor supporting gravity-aligned locomotion and optional thruster free-float mode.
/// </summary>
public sealed class SimpleCharacterMotor : ICharacterMotor
{
    private readonly float _walkAcceleration;
    private readonly float _maxWalkSpeed;
    private readonly float _jumpSpeed;
    private readonly float _thrusterAcceleration;
    private readonly float _orientationSharpness;
    private readonly float _zeroGravityThreshold;

    public SimpleCharacterMotor(
        CharacterMotorState initialState,
        float walkAcceleration = 40f,
        float maxWalkSpeed = 6f,
        float jumpSpeed = 7.5f,
        float thrusterAcceleration = 10f,
        float orientationSharpness = 12f,
        float zeroGravityThreshold = 0.05f)
    {
        if (walkAcceleration <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(walkAcceleration), walkAcceleration, "Walk acceleration must be positive.");
        }

        if (maxWalkSpeed <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(maxWalkSpeed), maxWalkSpeed, "Max walk speed must be positive.");
        }

        if (jumpSpeed <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(jumpSpeed), jumpSpeed, "Jump speed must be positive.");
        }

        if (thrusterAcceleration <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(thrusterAcceleration), thrusterAcceleration, "Thruster acceleration must be positive.");
        }

        if (orientationSharpness <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(orientationSharpness), orientationSharpness, "Orientation sharpness must be positive.");
        }

        if (zeroGravityThreshold < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(zeroGravityThreshold), zeroGravityThreshold, "Zero-g threshold cannot be negative.");
        }

        _walkAcceleration = walkAcceleration;
        _maxWalkSpeed = maxWalkSpeed;
        _jumpSpeed = jumpSpeed;
        _thrusterAcceleration = thrusterAcceleration;
        _orientationSharpness = orientationSharpness;
        _zeroGravityThreshold = zeroGravityThreshold;

        State = NormalizeState(initialState);
    }

    public CharacterMotorState State { get; private set; }

    public void SetGrounded(bool isGrounded)
        => State = State with { IsGrounded = isGrounded };

    public void SetThrusterMode(bool isEnabled)
        => State = State with { ThrusterModeEnabled = isEnabled };

    public void Simulate(float deltaSeconds, in CharacterMotorInput input, in Vector3 gravity)
    {
        if (deltaSeconds <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta time must be positive.");
        }

        var up = ResolveSmoothedUp(State.Up, gravity, deltaSeconds);
        var velocity = State.Velocity;

        if (State.ThrusterModeEnabled)
        {
            velocity += ClampMagnitude(input.ThrusterAxes, 1f) * (_thrusterAcceleration * deltaSeconds);
        }
        else
        {
            velocity += gravity * deltaSeconds;
            velocity = ApplyPlanarWalkAcceleration(velocity, up, input.MoveAxes, deltaSeconds);

            if (State.IsGrounded && input.JumpPressed)
            {
                velocity += up * _jumpSpeed;
                State = State with { IsGrounded = false };
            }
        }

        var position = State.Position + (velocity * deltaSeconds);
        State = State with
        {
            Position = position,
            Velocity = velocity,
            Up = up
        };
    }

    private static CharacterMotorState NormalizeState(CharacterMotorState state)
    {
        var up = state.Up.LengthSquared() > float.Epsilon
            ? Vector3.Normalize(state.Up)
            : Vector3.UnitY;

        return state with { Up = up };
    }

    private Vector3 ResolveSmoothedUp(Vector3 currentUp, Vector3 gravity, float deltaSeconds)
    {
        if (gravity.LengthSquared() <= _zeroGravityThreshold * _zeroGravityThreshold)
        {
            return currentUp;
        }

        var targetUp = -Vector3.Normalize(gravity);
        var blend = 1f - MathF.Exp(-_orientationSharpness * deltaSeconds);
        var blended = Vector3.Lerp(currentUp, targetUp, blend);

        return blended.LengthSquared() > float.Epsilon
            ? Vector3.Normalize(blended)
            : targetUp;
    }

    private Vector3 ApplyPlanarWalkAcceleration(Vector3 velocity, Vector3 up, Vector2 moveAxes, float deltaSeconds)
    {
        if (moveAxes.LengthSquared() <= float.Epsilon)
        {
            return velocity;
        }

        var planarInput = ClampMagnitude(new Vector3(moveAxes.X, 0f, moveAxes.Y), 1f);
        var forwardReference = MathF.Abs(Vector3.Dot(Vector3.UnitZ, up)) > 0.95f ? Vector3.UnitX : Vector3.UnitZ;
        var right = Vector3.Normalize(Vector3.Cross(forwardReference, up));
        var forward = Vector3.Normalize(Vector3.Cross(up, right));
        var desiredPlanarDirection = Vector3.Normalize((right * planarInput.X) + (forward * planarInput.Z));

        var planarVelocity = velocity - (Vector3.Dot(velocity, up) * up);
        planarVelocity += desiredPlanarDirection * (_walkAcceleration * deltaSeconds);

        if (planarVelocity.LengthSquared() > _maxWalkSpeed * _maxWalkSpeed)
        {
            planarVelocity = Vector3.Normalize(planarVelocity) * _maxWalkSpeed;
        }

        var verticalVelocity = Vector3.Dot(velocity, up) * up;
        return verticalVelocity + planarVelocity;
    }

    private static Vector3 ClampMagnitude(Vector3 value, float maxMagnitude)
    {
        var lengthSquared = value.LengthSquared();
        if (lengthSquared <= maxMagnitude * maxMagnitude || lengthSquared <= float.Epsilon)
        {
            return value;
        }

        return Vector3.Normalize(value) * maxMagnitude;
    }
}
