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

        ToggleUpdateOnce();
    }

    private async void ToggleUpdateOnce()
    {
        UpdateMode = UpdateModeEnum.Always;
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        UpdateMode = UpdateModeEnum.WhenMoved;
    }
}
