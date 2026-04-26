namespace Astroblock.World.Generation;

/// <summary>
/// Tunables for <see cref="SimpleNoiseWorldGenerator"/>.
/// Values are intentionally simple for the prototype phase.
/// </summary>
public sealed class SimpleNoiseWorldGeneratorConfig
{
    /// <summary>
    /// Probability gate (0..255) for whether a chunk attempts asteroid generation.
    /// Lower values produce emptier space.
    /// </summary>
    public byte ChunkSpawnThreshold { get; init; } = 8;

    /// <summary>
    /// Minimum asteroid radius (in voxels).
    /// </summary>
    public int MinAsteroidRadius { get; init; } = 5;

    /// <summary>
    /// Maximum asteroid radius (in voxels).
    /// </summary>
    public int MaxAsteroidRadius { get; init; } = 11;

    /// <summary>
    /// Surface perturbation amplitude (in voxels) applied via deterministic noise.
    /// </summary>
    public float SurfaceNoiseAmplitude { get; init; } = 1.25f;

    /// <summary>
    /// Frequency multiplier for deterministic surface noise sampling.
    /// </summary>
    public float SurfaceNoiseFrequency { get; init; } = 0.12f;

    /// <summary>
    /// Voxel id used for generated solid matter.
    /// </summary>
    public byte SolidBlockValue { get; init; } = 1;

    internal void Validate()
    {
        if (MinAsteroidRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinAsteroidRadius), MinAsteroidRadius, "Minimum radius must be greater than zero.");
        }

        if (MaxAsteroidRadius < MinAsteroidRadius)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxAsteroidRadius), MaxAsteroidRadius, "Maximum radius must be greater than or equal to minimum radius.");
        }

        if (SurfaceNoiseAmplitude < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(SurfaceNoiseAmplitude), SurfaceNoiseAmplitude, "Noise amplitude cannot be negative.");
        }

        if (SurfaceNoiseFrequency <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(SurfaceNoiseFrequency), SurfaceNoiseFrequency, "Noise frequency must be greater than zero.");
        }
    }
}
