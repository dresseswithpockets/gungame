using System;
using Godot;

[Flags]
public enum AxisMask
{
    X = 1 << 0,
    Y = 1 << 1,
    Z = 1 << 2,
}
