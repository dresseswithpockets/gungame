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

    [Signal]
    public delegate void TriggerEventHandler(Node3D activator);

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;
        
        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;

        shouldPreserveMomentum = properties.GetOrDefault("preserve_momentum", false);
        
        var targetPointName = properties.GetOrDefault("target", (string)null);
        if (string.IsNullOrEmpty(targetPointName))
        {
            GD.PushWarning($"'{Name}' has no target portal, so it will not link to anything.");
            return;
        }

        var targetPoints = qodotMap.Call("get_nodes_by_targetname", targetPointName).AsGodotObjectArray<Node>();
        switch (targetPoints.Length)
        {
            case 0:
                GD.PushWarning(
                    $"'{Name}' targets '{targetPointName}', but there are no entities with that name, so it will not link to anything.");
                return;
            case > 1:
                GD.PushWarning($"'{Name}' targets multiple entities named '{targetPointName}'. Will only link to the first one.");
                break;
        }

        var targetPoint = targetPoints[0];
        if (targetPoint == this)
        {
            GD.PushError($"'{Name}' targets itself, but must target a different entity.");
            return;
        }

        if (targetPoint is not Node3D targetPointNode)
        {
            GD.PushError($"'{Name}' must target a Node3D-derived entity, but it targets '{targetPointName}', which doesn't derive from Node3D.");
            return;
        }

        targetNode = targetPointNode;
    }

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
            BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
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
}

internal interface ITeleportTraveller
{
    void TeleportTo(Node3D targetNode);
}
