using Godot;
using System.Collections.Generic;
using Godot.Collections;
using GunGame;

[Tool]
public partial class TriggerGravity : Area3D
{
    [Export] public Dictionary properties;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;

        Gravity = properties.GetOrDefault("gravity", 0f);
        GravitySpaceOverride = SpaceOverride.Replace;
    }
}
