using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class EnvDrip : GpuParticles3D
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

        var meshInstance = this.FindFirstImmediateChild<MeshInstance3D>();
        var extents = Vector3.Zero;
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
        processMaterial.CollisionMode = ParticleProcessMaterial.CollisionModeEnum.HideOnContact;
        ProcessMaterial = processMaterial;
        DrawPasses = 1;
        DrawPass1 = GD.Load<Mesh>("res://entities/env_drip/env_drip_pass1.tres");
        Lifetime = properties.GetOrDefault("lifetime", 10f);
        Amount = properties.GetOrDefault("amount", 16);
        Emitting = properties.GetOrDefault("emit_on_spawn", true);
        OneShot = properties.GetOrDefault("one_shot", false);
    }

    // ReSharper disable once InconsistentNaming
    public void use(Node3D _)
    {
        Emitting = !Emitting;
    }
}
