using Godot.Collections;

namespace GunGame;

public static class GodotCollectionsExtensions
{
    public static int GetOrDefault(this Dictionary dictionary, string key, int @default)
        => dictionary.TryGetValue(key, out var value) ? value.AsInt32() : default;
}
