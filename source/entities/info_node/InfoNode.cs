using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class InfoNode : Node3D
{
    [Export] public Dictionary properties;
    [Export] public InfoNode targetNode;

    public void UpdateProperties(Node3D qodotMap)
    {
        var target = properties.GetOrDefault("target", "");
        if (string.IsNullOrWhiteSpace(target)) return;
        targetNode = GetFirstNode(qodotMap, target);
    }

    private InfoNode GetFirstNode(Node3D qodotMap, string nodeName)
    {
        var nodes = qodotMap.Call("get_nodes_by_targetname", nodeName).AsGodotObjectArray<Node>();
        switch (nodes.Length)
        {
            case 0:
                GD.PushWarning(
                    $"'{Name}' targets '{nodeName}', but there are no entities with that name, so it will not link to anything.");
                return null;
            case > 1:
                GD.PushWarning($"'{Name}' targets multiple entities named '{nodeName}'. Will only link to the first one.");
                break;
        }

        var node = nodes[0];
        if (node == this)
        {
            GD.PushError($"'{Name}' targets itself, but must target a different entity.");
            return null;
        }

        if (node is not InfoNode infoNode)
        {
            GD.PushError($"'{Name}' must target a InfoNode-derived entity, but it targets '{nodeName}', which doesn't derive from InfoNode.");
            return null;
        }

        return infoNode;
    }
}
