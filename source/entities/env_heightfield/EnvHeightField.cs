using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class EnvHeightField : GpuParticlesCollisionHeightField3D
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
