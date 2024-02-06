using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class FuncRotateIllusionary : Node3D
{
    [Export] public Dictionary properties;

    [Export] public Vector3 axis;
    [Export] public float speed;
    [Export] public Node3D target;
    [Export] public bool startRandom;
    
    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        axis = properties.GetOrDefault("angles", Vector3.Zero);
        axis = QodotMath.TbAnglesToDirection(axis);
        speed = properties.GetOrDefault("speed", 0f);
        speed = Mathf.DegToRad(speed);
        startRandom = properties.GetOrDefault("start_random", false);

        var targetName = properties.GetOrDefault("target", "");
        if (!string.IsNullOrWhiteSpace(targetName) && !qodotMap.QodotMapOneOrNull(Name, targetName, out target))
        {
            GD.PushWarning(
                $"'{Name}' has a target '{targetName}' but there is no entity by that name, so '{Name}' will rotate around its origin instead.");
        }
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        
        if (startRandom)
            DoRotation((float)GD.RandRange(0f, Mathf.Pi));
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        DoRotation(speed * (float)delta);
    }

    private void DoRotation(float amount)
    {        
        if (target != null)
        {
            GlobalPosition = target.GlobalPosition +
                             (GlobalPosition - target.GlobalPosition).Rotated(axis, amount);
        }
        else
        {
            Rotate(axis, amount);
        }
    }
}
