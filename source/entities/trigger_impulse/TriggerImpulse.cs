using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class TriggerImpulse : Area3D
{
    [Export] public Dictionary properties;

    [Export] public Vector3 amount;
    [Export] public AxisMask axisMaskFlags;
    [Export] public bool overrideVelocity;
    [Export] public bool onlyOnce;

    private bool _ranOnce;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;

        amount = properties.GetOrDefault("amount", Vector3.Zero);
        axisMaskFlags = (AxisMask)properties.GetOrDefault("axis_mask", (int)AxisMask.Y);
        overrideVelocity = properties.GetOrDefault("override_velocity", false);
        onlyOnce = properties.GetOrDefault("only_once", false);
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (onlyOnce && _ranOnce)
            return;

        switch (body)
        {
            case RigidBody3D rigidBody:
                ImpulseRigidBody(rigidBody);
                break;
            case IPushable pushable:
                ImpulsePushable(pushable);
                break;
        }
    }

    private void ImpulsePushable(IPushable pushable)
    {
        if (overrideVelocity)
        {
            var velocity = pushable.Velocity;
            axisMaskFlags.ApplyMaskedVector(ref velocity, ref amount);
            pushable.OverrideVelocity(velocity);
        }
        else
        {
            pushable.AddImpulse(amount);
        }

        if (!onlyOnce) return;
        
        _ranOnce = true;
        var collisionShape = this.FindFirstImmediateChild<CollisionShape3D>();
        collisionShape.Disabled = true;
    }

    private void ImpulseRigidBody(RigidBody3D rigidBody)
    {
        if (overrideVelocity)
        {
            var velocity = rigidBody.LinearVelocity;
            axisMaskFlags.ApplyMaskedVector(ref velocity, ref amount);
            rigidBody.LinearVelocity = velocity;
        }
        else
        {
            rigidBody.ApplyImpulse(amount);                
        }

        if (!onlyOnce) return;
        
        _ranOnce = true;
        var collisionShape = this.FindFirstImmediateChild<CollisionShape3D>();
        collisionShape.Disabled = true;
    }
}
