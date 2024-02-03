using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class LogicRelay : Node
{
    [Signal] public delegate void TriggerEventHandler(Node3D activator);
    
    [Export] public Dictionary properties;

    [Export] public bool startDisabled;
    [Export] public int onlyN;

    private bool _enabled;
    private int _fireCount;

    public override void _Ready() => _enabled = !startDisabled;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        startDisabled = properties.GetOrDefault("start_disabled", false);
        onlyN = properties.GetOrDefault("only_n", 0);
    }

    // ReSharper disable once InconsistentNaming
    public void use(Node3D activator)
    {
        if (!_enabled) return;
        if (onlyN > 0 && _fireCount >= onlyN) return;

        EmitSignal(SignalName.Trigger, activator);
        _fireCount++;
    }

    // ReSharper disable once InconsistentNaming
    public void enable(Node3D _) => _enabled = true;

    // ReSharper disable once InconsistentNaming
    public void disable(Node3D _) => _enabled = false;

    // ReSharper disable once InconsistentNaming
    public void reset(Node3D _)
    {
        _enabled = false;
        _fireCount = 0;
    }
}