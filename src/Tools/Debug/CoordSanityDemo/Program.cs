using Astroblock.Core.Coords;

var samples = new (int X, int Y, int Z)[]
{
    (0, 0, 0),
    (31, 31, 31),
    (32, 32, 32),
    (-1, -1, -1),
    (-32, -32, -32),
    (-33, 63, -64),
    (int.MinValue, 0, int.MaxValue)
};

Console.WriteLine("CoordSanityDemo - world/chunk/local round-trip checks");
Console.WriteLine();

foreach (var world in samples)
{
    var chunk = CoordConverter.WorldToChunk(world.X, world.Y, world.Z);
    var local = CoordConverter.WorldToLocal(world.X, world.Y, world.Z);
    var reconstructed = CoordConverter.ChunkLocalToWorld(chunk, local.X, local.Y, local.Z);

    var passed = reconstructed.X == world.X && reconstructed.Y == world.Y && reconstructed.Z == world.Z;
    var marker = passed ? "PASS" : "FAIL";

    Console.WriteLine($"[{marker}] world=({world.X}, {world.Y}, {world.Z})");
    Console.WriteLine($"       chunk=({chunk.X}, {chunk.Y}, {chunk.Z}) local=({local.X}, {local.Y}, {local.Z})");
    Console.WriteLine($"       reconstructed=({reconstructed.X}, {reconstructed.Y}, {reconstructed.Z})");
    Console.WriteLine();
}
