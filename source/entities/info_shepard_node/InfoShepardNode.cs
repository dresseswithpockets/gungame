using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class InfoShepardNode : Node3D
{
    [Export] public Dictionary properties;
    [Export] public Array<Node3D> runAwayNodes;

    public void UpdateProperties(Node3D qodotMap)
    {
        qodotMap.QodotMapGetAllTargets(Name, properties, ref runAwayNodes);
    }
}
