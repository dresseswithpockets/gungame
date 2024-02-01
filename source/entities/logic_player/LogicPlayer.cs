using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class LogicPlayer : Node
{
    [Export] public Dictionary properties;
    [Export] public SetAllowMovement setAllowMovement;
    [Export] public SetAllowGrapple setAllowGrapple;

    public void UpdateProperties(Node3D _)
    {
        setAllowMovement = (SetAllowMovement)properties.GetOrDefault("set_allow_movement", (int)SetAllowMovement.No);
        setAllowGrapple = (SetAllowGrapple)properties.GetOrDefault("set_allow_grapple", (int)SetAllowGrapple.No);
    }

    // ReSharper disable once InconsistentNaming
    public void use(Node3D activator) => Use(activator);
    
    public void Use(Node3D activator)
    {
        if (activator is not Player player) return;

        if (setAllowMovement != SetAllowMovement.No)
            player.allowMovement = setAllowMovement == SetAllowMovement.Allow;

        if (setAllowGrapple != SetAllowGrapple.No)
            player.allowGrapple = setAllowGrapple == SetAllowGrapple.Allow;
    }
    
    public enum SetAllowMovement
    {
        No = 0,
        Allow = 1,
        Disallow = 2
    }
    
    public enum SetAllowGrapple
    {
        No = 0,
        Allow = 1,
        Disallow = 2
    }
}
