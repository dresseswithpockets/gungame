using Godot;
using GunGame;

[Tool]
public partial class LogicRelay : QodotBaseEntity
{
    [Signal]
    // ReSharper disable once InconsistentNaming
    public delegate void triggerEventHandler(Node3D activator);

    [Export] public bool startDisabled;
    [Export] public bool onlyOnce;

    private bool _enabled;
    private bool _fired;

    public override void _Ready() => _enabled = !startDisabled;
    
    protected override void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        startDisabled = properties.GetOrDefault("start_disabled", false);
        onlyOnce = properties.GetOrDefault("only_once", false);
    }

    // ReSharper disable once InconsistentNaming
    public void use(Node3D activator)
    {
        if (!_enabled) return;
        if (onlyOnce && _fired) return;
        
        EmitSignal(SignalName.trigger, activator);
        _fired = true;
    }

    // ReSharper disable once InconsistentNaming
    public void enable(Node3D _) => _enabled = true;

    // ReSharper disable once InconsistentNaming
    public void disable(Node3D _) => _enabled = false;
}
