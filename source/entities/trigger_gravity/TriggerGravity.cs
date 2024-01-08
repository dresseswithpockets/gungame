using Godot;
using System.Collections.Generic;
using Godot.Collections;
using GunGame;

[Tool]
public partial class TriggerGravity : Area3D
{
    private Dictionary _properties;
    // the gdscript side expects lowercase properties :(
    // ReSharper disable once InconsistentNaming
    [Export]
    public Dictionary properties
    {
        get => _properties;
        set
        {
            if (_properties == value) return;
            _properties = value;
            UpdateProperties();
        }
    }

    private void UpdateProperties()
    {
        if (!Engine.IsEditorHint())
            return;

        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;

        Gravity = properties.GetOrDefault("gravity", 0f);
        GravitySpaceOverride = SpaceOverride.Replace;
    }
}
