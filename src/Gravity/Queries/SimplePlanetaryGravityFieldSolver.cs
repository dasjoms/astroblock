using System.Numerics;

namespace Astroblock.Gravity.Queries;

/// <summary>
/// First-pass gravity solver for a single planetary mass source.
/// </summary>
public sealed class SimplePlanetaryGravityFieldSolver : IGravityFieldSolver
{
    private readonly float _gravitationalParameter;
    private readonly float _minimumDistanceSquared;

    public SimplePlanetaryGravityFieldSolver(Vector3 sourcePosition, float gravitationalParameter, float minimumDistance = 1f)
    {
        if (gravitationalParameter <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(gravitationalParameter), gravitationalParameter, "Gravitational parameter must be positive.");
        }

        if (minimumDistance <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumDistance), minimumDistance, "Minimum distance must be positive.");
        }

        SourcePosition = sourcePosition;
        _gravitationalParameter = gravitationalParameter;
        _minimumDistanceSquared = minimumDistance * minimumDistance;
    }

    /// <summary>
    /// Planet center-of-mass in world space.
    /// </summary>
    public Vector3 SourcePosition { get; }

    public Vector3 SampleGravity(in Vector3 worldPosition)
    {
        var toSource = SourcePosition - worldPosition;
        var distanceSquared = toSource.LengthSquared();

        if (distanceSquared <= float.Epsilon)
        {
            return Vector3.Zero;
        }

        var clampedDistanceSquared = MathF.Max(distanceSquared, _minimumDistanceSquared);
        var directionToSource = Vector3.Normalize(toSource);
        var strength = _gravitationalParameter / clampedDistanceSquared;

        return directionToSource * strength;
    }
}
