using System.Numerics;

namespace Astroblock.Gravity.Queries;

/// <summary>
/// Samples the local gravity field at a world-space position.
/// </summary>
public interface IGravityFieldSolver
{
    /// <summary>
    /// Returns the gravity acceleration vector at <paramref name="worldPosition"/>.
    /// </summary>
    Vector3 SampleGravity(in Vector3 worldPosition);
}
