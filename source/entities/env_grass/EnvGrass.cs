using Godot;
using Godot.Collections;

[Tool]
public partial class EnvGrass : Node3D
{
    [Export] public Dictionary properties;
    [Export(PropertyHint.Layers3DPhysics)] public uint alignmentCollisionMask = 1;

    public static Basis AlignUp(Basis basis, Vector3 normal)
    {
        var result = new Basis
        {
            X = normal.Cross(basis.Z),
            Y = normal,
            Z = basis.X.Cross(normal)
        };

        return result.Orthonormalized();
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        var rayQueryParameters = new PhysicsRayQueryParameters3D
        {
            From = GlobalPosition + new Vector3(0f, 0.5f, 0f),
            To = GlobalPosition + new Vector3(0f, -0.5f, 0f),
            CollideWithBodies = true,
            CollideWithAreas = false,
            CollisionMask = alignmentCollisionMask,
        };
        
        var result = GetWorld3D().DirectSpaceState.IntersectRay(rayQueryParameters);
        var normal = Vector3.Up;
        if (result.Count > 0)
        {
            var hitOrigin = result["position"].AsVector3();
            normal = result["normal"].AsVector3();
            var normalAlignedBasis = AlignUp(GlobalBasis, normal);
            GlobalTransform = new Transform3D(normalAlignedBasis, hitOrigin);
        }
        
        Rotate(normal, (float)GD.RandRange(0, Mathf.Pi));
    }
}
