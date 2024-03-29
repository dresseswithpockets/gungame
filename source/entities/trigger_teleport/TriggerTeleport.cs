using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class TriggerTeleport : Area3D
{
    [Export]
    public Dictionary properties;

    [Export] public Node3D targetNode;
    [Export] public bool shouldPreserveMomentum;
    [Export] public int disableAfter;

    private int _fireCount;

    [Signal]
    public delegate void TriggerEventHandler(Node3D activator);

    // TODO: add an option for axis flags e.g. only teleport the Y position
    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;
        
        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;

        shouldPreserveMomentum = properties.GetOrDefault("preserve_momentum", false);
        disableAfter = properties.GetOrDefault("disable_after", 0);
        
        var targetPointName = properties.GetOrDefault("target", "");
        if (string.IsNullOrWhiteSpace(targetPointName))
        {
            GD.PushWarning($"'{Name}' has no target point, so it will not link to anything.");
            return;
        }

        qodotMap.QodotMapOneOrNull(Name, targetPointName, out targetNode);
    }

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
            BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (disableAfter > 0 && _fireCount >= disableAfter) return;
        
        _fireCount++;
        EmitSignal(SignalName.Trigger, body);
        switch (body)
        {
            // some bodies - like the player - need to do some special stuff when teleporting, like preserving momentum
            // in the direction of the teleport
            case ITeleportTraveller traveller:
                traveller.TeleportTo(targetNode);
                return;
            case RigidBody3D rigidBody:
                // TODO: do we even *need* rigidbody? Maybe for projectiles and props...
                // reorient the rigidbody's velocity
                rigidBody.LinearVelocity = -targetNode.GlobalTransform.Basis.Z * rigidBody.LinearVelocity.Length();
            
                // TODO: RigidBody3D doesnt work well with direct manipulation like setting GlobalPosition/GlobalRotation
                //       so, it would be best to override _IntegrateState in a custom class that we default to using instead
                //       of RigidBody3D. This direct manipulation could instead be a fallback.
                rigidBody.ConstantForce = -targetNode.GlobalTransform.Basis.Z * rigidBody.ConstantForce.Length();
                break;
        }

        // we dont assign GlobalTransform, just in case targetNode has a non-(1,1,1) scale for some ungodly reason
        body.GlobalPosition = targetNode.GlobalPosition;
        body.GlobalRotation = targetNode.GlobalRotation;
    }

    // ReSharper disable once InconsistentNaming
    public void reset(Node3D _)
    {
        _fireCount = 0;
    }
}

internal interface ITeleportTraveller
{
    void TeleportTo(Node3D targetNode);
}
