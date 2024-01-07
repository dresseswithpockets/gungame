using System;
using Godot;

[Flags]
public enum AxisMask
{
    X = 1 << 0,
    Y = 1 << 1,
    Z = 1 << 2,
}

public static class AxisMaskExtensions
{
    public static void ApplyMaskedVector(this AxisMask mask, ref Vector3 target, ref Vector3 amount)
    {
        if (mask.HasFlag(AxisMask.X))
            target.X = amount.X;

        if (mask.HasFlag(AxisMask.Y))
            target.Y = amount.Y;

        if (mask.HasFlag(AxisMask.Z))
            target.Z = amount.Z;
    }
}