using Godot;

namespace GunGame;

public partial class PlayerInventory : Node
{
    public readonly bool[] keys = new bool[4];
    
    [Signal]
    public delegate void KeyAddedEventHandler(ItemKeyColor color);
    
    public void AddKey(ItemKeyColor color)
    {
        keys[(int)color] = true;
        EmitSignal(SignalName.KeyAdded, (int)color);
    }
}