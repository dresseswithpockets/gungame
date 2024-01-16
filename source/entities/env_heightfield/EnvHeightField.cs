using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class EnvHeightField : GpuParticlesCollisionHeightField3D
{
    [Export] public Dictionary properties;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        var meshInstance = this.FindFirstImmediateChild<MeshInstance3D>();
        if (meshInstance != null && IsInstanceValid(meshInstance))
        {
            //Position = meshInstance.GetAabb().Position;
            Size = meshInstance.GetAabb().Size;
            RemoveChild(meshInstance);
            meshInstance.QueueFree();
        }

        // immediately gets set back to WhenMoved next frame
        UpdateMode = UpdateModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() && UpdateMode == UpdateModeEnum.Always)
            UpdateMode = UpdateModeEnum.WhenMoved;
    }
}
