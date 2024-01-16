using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class EnvParticles : GpuParticles3D
{
    [Export] public Dictionary properties;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        var useName = properties.GetOrDefault("targetname", (string)null);
        if (string.IsNullOrEmpty(useName))
            useName = Name;
        
        var particlePassName = properties.GetOrDefault("particle", "particles/env_drip_pass1.tres");
        if (!particlePassName.StartsWith("res://"))
            particlePassName = $"res://{particlePassName}";
        if (!ResourceLoader.Exists(particlePassName, nameof(Mesh)))
        {
            GD.PushError($"'{useName}' points to a non-existent particle asset '{particlePassName}'");
            // TODO: warn in console?
            return;
        }

        var meshInstance = this.FindFirstImmediateChild<MeshInstance3D>();
        Vector3 extents;
        if (meshInstance == null || !IsInstanceValid(meshInstance))
        {
            if (ProcessMaterial is not ParticleProcessMaterial particleProcessMaterial)
            {
                GD.PushError(
                    "Couldn't build env_particles because it has no MeshInstance to retrieve AABB from, and no ProcessMaterial to get existing extents from.");
                return;
            }

            extents = particleProcessMaterial.EmissionBoxExtents;
        }
        else
        {
            extents = meshInstance.GetAabb().Size * 0.5f;
            RemoveChild(meshInstance);
            meshInstance.QueueFree();
        }
        
        var angles = properties.GetOrDefault("angles", Vector3.Zero);
        angles.X = Mathf.DegToRad(-angles.X);
        angles.Y = Mathf.DegToRad(angles.Y + 180);
        angles.Z = Mathf.DegToRad(angles.Z);
        var direction = Basis.FromEuler(angles) * Vector3.Forward;
        
        var processMaterial = new ParticleProcessMaterial();
        processMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
        processMaterial.EmissionBoxExtents = extents;
        processMaterial.Direction = direction;
        processMaterial.Spread = properties.GetOrDefault("spread", 10f);
        processMaterial.InitialVelocityMin = properties.GetOrDefault("speed_min", 0.1f);
        processMaterial.InitialVelocityMax = properties.GetOrDefault("speed_max", 0.2f);
        processMaterial.CollisionMode =
            (ParticleProcessMaterial.CollisionModeEnum)properties.GetOrDefault("collision_mode",
                (int)ParticleProcessMaterial.CollisionModeEnum.Disabled);
        processMaterial.Gravity = properties.GetOrDefault("gravity", Vector3.Down * -10f);
        ProcessMaterial = processMaterial;
        DrawPasses = 1;
        DrawPass1 = GD.Load<Mesh>(particlePassName);
        Lifetime = properties.GetOrDefault("lifetime", 10f);
        Emitting = properties.GetOrDefault("emit_on_spawn", true);
        OneShot = properties.GetOrDefault("one_shot", false);
        
        var density = properties.GetOrDefault("density", 0f);
        if (density == 0)
            Amount = properties.GetOrDefault("amount", 16);
        else
            Amount = (int)(density * extents.X * extents.Y * 2f);
    }

    // ReSharper disable once InconsistentNaming
    public void use(Node3D _)
    {
        Emitting = !Emitting;
    }
}
