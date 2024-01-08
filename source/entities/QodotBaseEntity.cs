using Godot;
using Godot.Collections;

[GlobalClass]
public abstract partial class QodotBaseEntity : Node
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

    protected abstract void UpdateProperties();
}