namespace Astroblock.World.Generation;

/// <summary>
/// Tunables for <see cref="SimplePlanetWorldGenerator"/>.
/// </summary>
public sealed class SimplePlanetWorldGeneratorConfig
{
    /// <summary>
    /// Planet radius in voxels.
    /// </summary>
    public int PlanetRadius { get; init; } = 192;

    /// <summary>
    /// Low-amplitude surface perturbation amount in voxels.
    /// </summary>
    public float SurfaceNoiseAmplitude { get; init; } = 2.0f;

    /// <summary>
    /// Frequency multiplier for deterministic surface perturbation sampling.
    /// </summary>
    public float SurfaceNoiseFrequency { get; init; } = 0.03f;

    /// <summary>
    /// Solid voxel value to fill planet volume.
    /// </summary>
    public byte SolidBlockValue { get; init; } = 1;

    internal void Validate()
    {
        if (PlanetRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(PlanetRadius), PlanetRadius, "Planet radius must be greater than zero.");
        }

        if (SurfaceNoiseAmplitude < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(SurfaceNoiseAmplitude), SurfaceNoiseAmplitude, "Surface noise amplitude cannot be negative.");
        }

        if (SurfaceNoiseFrequency <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(SurfaceNoiseFrequency), SurfaceNoiseFrequency, "Surface noise frequency must be greater than zero.");
        }
    }
}
