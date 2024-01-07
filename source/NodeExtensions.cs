using Godot;

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
}