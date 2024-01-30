using System.Diagnostics;
using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class TriggerHurt : Area3D
{
    [Export] public Dictionary properties;
    [Export] public int amount;

    public void UpdateProperties(Node3D _)
    {
        if (!Engine.IsEditorHint())
            return;
        
        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;

        amount = properties.GetOrDefault("amount", 1);
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is not IDamageable damageable) return;
        damageable.Damage(amount);
    }
}
