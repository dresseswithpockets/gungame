using Godot;
using System;

[GlobalClass]
public partial class CommandData : GodotObject
{
    public readonly string name;
    public readonly Callable function;
    
    public CommandData(string name, Callable function)
    {
        this.name = name.ToLower();
        this.function = function;
    }
}
