using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public partial class PlayerGrappleHook : Node3D
{
    [Export] public float forwardSpeed = 10f;
    [Export(PropertyHint.Layers3DPhysics)] public uint collisionMask;
    [Export] public MeshInstance3D ropeMesh;
    [Export] public Node3D ropeOrigin;
    [Export] public float ropeScaleFactor = 10f;

    public Player player;

    private CollisionObject3D _collider;

    public bool IsPulling => _collider != null && IsInstanceValid(_collider);

    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_collider == null)
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
                _collider = intersectionResult.GetValueOrDefault("collider", (CollisionObject3D)null)
                    .As<CollisionObject3D>();
            }
            else
            {
                GlobalPosition = ray.To;
            }
        }

        if (player != null && IsInstanceValid(player))
        {
            ropeMesh.GlobalPosition = ropeOrigin.GlobalPosition.Lerp(player.GlobalPosition, 0.5f);
            var scaleZ = (player.GlobalPosition - ropeOrigin.GlobalPosition).Length() * ropeScaleFactor;
            ropeMesh.Scale = ropeMesh.Scale with { Z = scaleZ };
            ropeMesh.LookAt(ropeOrigin.GlobalPosition);
        }
    }
}
