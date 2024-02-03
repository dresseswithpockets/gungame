using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class LogicChance : Node
{
    [Export] public Dictionary properties;
    
    [Signal]
    // ReSharper disable once InconsistentNaming
    public delegate void triggerEventHandler(Node3D activator);
    
    [Export] public float chance;

    // shared across all logic_chances
    private static readonly RandomNumberGenerator NumberGenerator = new();

    public void UpdateProperties(Node3D qodotMap)
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
