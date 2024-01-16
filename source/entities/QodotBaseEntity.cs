using Godot;
using Godot.Collections;

[GlobalClass]
public abstract partial class QodotBaseEntity : Node
{
    [Export] public Dictionary properties;

    protected abstract void UpdateProperties(Node3D qodotMap);
}