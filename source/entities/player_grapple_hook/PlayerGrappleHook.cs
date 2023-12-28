using System.Collections.Generic;
using Godot;

public partial class PlayerGrappleHook : Node3D
{
    [Export] public float forwardSpeed = 10f;
    [Export(PropertyHint.Layers3DPhysics)] public uint collisionMask;

    public Player player;
    
    public override void _PhysicsProcess(double delta)
    {
        var ray = new PhysicsRayQueryParameters3D();
        // TODO: collide with areas and bodies (expensive, but necessary for some triggers and character interactions)
        ray.CollisionMask = collisionMask;
        ray.From = GlobalPosition;
        ray.To = GlobalPosition + (-GlobalTransform.Basis.Z * forwardSpeed * (float)delta);
        var intersectionResult = GetWorld3D().DirectSpaceState.IntersectRay(ray);
        if (intersectionResult.Count != 0)
        {
            var intersectionPosition = intersectionResult.GetValueOrDefault("position", Vector3.Zero).AsVector3();
            GlobalPosition = intersectionPosition;
            SetPhysicsProcess(false);
        }
    }
}
