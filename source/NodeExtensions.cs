using System;
using Godot;
using Godot.Collections;

namespace GunGame;

public static class NodeExtensions
{
    public static T FindFirstChild<T>(this Node node) where T : Node
    {
        foreach (var child in node.GetChildren())
        {
            if (child is T t)
                return t;
            
            var result = child.FindFirstChild<T>();
            if (result != null)
                return result;
        }

        return null;
    }

    public static T FindFirstImmediateChild<T>(this Node node) where T : Node
    {
        foreach (var child in node.GetChildren())
        {
            if (child is T t)
                return t;
        }

        return null;
    }
    
    public static bool QodotMapOneOrNull<T>(this GodotObject qodotMap, string sourceName, string targetName, out T entity) where T : Node
    {
        var nodes = qodotMap.Call("get_nodes_by_targetname", targetName).AsGodotObjectArray<Node>();
        switch (nodes.Length)
        {
            case 0:
                GD.PushWarning(
                    $"'{sourceName}' targets '{targetName}', but there are no entities with that name, so it will not link to anything.");
                entity = null;
                return false;
            case > 1:
                GD.PushWarning($"'{sourceName}' targets multiple entities named '{targetName}'. Will only link to the first one.");
                break;
        }

        var one = nodes[0];
        if (one is not T t)
        {
            GD.PushWarning($"'{sourceName}' expects a {typeof(T).Name}-derived type, but '{targetName}' doesn't derive from {typeof(T).Name}.");
            entity = null;
            return false;
        }

        entity = t;
        return true;
    }

    public static void QodotMapGetAllTargets<[MustBeVariant] T>(
        this GodotObject qodotMap,
        string sourceName,
        Dictionary properties,
        ref Array<T> array)
        where T : Node
    {
        if (array == null)
            array = new Array<T>();
        else
            array.Clear();
        
        var target = properties.GetOrDefault("target", "");
        if (!string.IsNullOrWhiteSpace(target) && QodotMapOneOrNull(qodotMap, sourceName, target, out T t))
            array.Add(t);

        foreach (var (keyVariant, value) in properties)
        {
            var key = keyVariant.AsString();
            if (!key.StartsWith("target") || key == "target") continue;
            if (!int.TryParse(key.AsSpan(6), out _)) continue;
            if (QodotMapOneOrNull(qodotMap, sourceName, value.AsString(), out t))
                array.Add(t);
        }
    }
}