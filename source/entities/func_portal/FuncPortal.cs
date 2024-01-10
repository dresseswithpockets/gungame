using Godot;
using Godot.Collections;
using GunGame;

public partial class FuncPortal : Area3D
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
    }
}
