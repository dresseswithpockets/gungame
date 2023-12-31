using Godot;
using GunGame;

[Tool]
public partial class LogicChance : QodotBaseEntity
{
    [Signal]
    // ReSharper disable once InconsistentNaming
    public delegate void triggerEventHandler(Node3D activator);
    
    [Export] public float chance;

    // shared across all logic_chances
    private static readonly RandomNumberGenerator NumberGenerator = new();
    
    protected override void UpdateProperties()
    {
        if (!Engine.IsEditorHint())
            return;

        chance = properties.GetOrDefault("chance", 0.5f);
    }

    // ReSharper disable once InconsistentNaming
    public void use(Node3D activator)
    {
        if (NumberGenerator.Randf() < chance)
            EmitSignal(SignalName.trigger, activator);
    }
}
