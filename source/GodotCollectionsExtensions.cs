using System;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

namespace GunGame;

public static class GodotCollectionsExtensions
{
    public static int GetOrDefault(this Dictionary dictionary, string key, int @default = default)
        => dictionary.TryGetValue(key, out var value) ? value.AsInt32() : @default;
    
    public static float GetOrDefault(this Dictionary dictionary, string key, float @default = default)
        => dictionary.TryGetValue(key, out var value) ? value.AsSingle() : @default;
    
    public static bool GetOrDefault(this Dictionary dictionary, string key, bool @default = default)
        => dictionary.TryGetValue(key, out var value) ? value.AsBool() : @default;
    
    public static Vector3 GetOrDefault(this Dictionary dictionary, string key, Vector3 @default = default)
        => dictionary.TryGetValue(key, out var value) ? value.AsVector3() : @default;
    
    public static string GetOrDefault(this Dictionary dictionary, string key, string @default = "")
        => dictionary.TryGetValue(key, out var value) ? value.AsString() : @default;
}
