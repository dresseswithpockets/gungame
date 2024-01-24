using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class FuncRotate : AnimatableBody3D
{
    [Export] public Dictionary properties;

    [Export] public Vector3 axis;
    [Export] public float speed;
    
    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        axis = properties.GetOrDefault("angles", Vector3.Zero);
        axis = QodotMath.TbAnglesToDirection(axis);
        speed = properties.GetOrDefault("speed", 0f);
        speed = Mathf.DegToRad(speed);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;
        
        Rotate(axis, speed * (float)delta);
    }
}
