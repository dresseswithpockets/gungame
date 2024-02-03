using Godot;

public static class QodotMath
{
    public static Vector3 TbAnglesToDirection(Vector3 angles)
    {
        angles.X = Mathf.DegToRad(-angles.X);
        angles.Y = Mathf.DegToRad(angles.Y + 180);
        angles.Z = Mathf.DegToRad(angles.Z);
        return Basis.FromEuler(angles) * Vector3.Forward;
    }

    public static float TbUnitsToGodot(float amount) => amount / 128f;
}