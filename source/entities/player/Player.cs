using Godot;
using System;

public partial class Player : CharacterBody3D
{
    public const float Accel = 150f;
    public const float Decel = 50f;
    public const float Speed = 10.0f;
    public const float BoostSpeedMultiplier = 1.15f;
    public const float JumpVelocity = 4.5f;
    public const float YawSpeed = 0.022f;
    public const float PitchSpeed = 0.022f;
    public const float Sensitivity = 2f;
    public static readonly Vector3 CameraBob = new(0f, -0.15f, 0f);
    public const float CameraBobSpeed = 3f;
    public const float CameraBobResetSpeed = 10f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private Camera3D _camera;
    private Vector3 _cameraStart;
    private float _cameraBobTimer;
    private bool _cameraGoingDown = true;

    // velocity without any Y component
    private Vector3 _groundVelocity;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _cameraStart = _camera.Position;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (@event is not InputEventMouseMotion mouseMotion) return;        
        RotateY(Mathf.DegToRad(Sensitivity * YawSpeed * -mouseMotion.Relative.X));
        _camera.RotateX(Mathf.DegToRad(Sensitivity * PitchSpeed * -mouseMotion.Relative.Y));
        // clamping camera's pitch to +/- 90 to prevent inversion
        var rot = _camera.RotationDegrees;
        rot.X = Mathf.Clamp(rot.X, -90, 90);
        _camera.RotationDegrees = rot;
    }

    public override void _PhysicsProcess(double delta)
    {
        var verticalSpeed = Velocity.Y;
        var deltaF = (float)delta;

        // Add the gravity.
        if (!IsOnFloor())
            verticalSpeed -= gravity * deltaF;

        // Handle Jump.
        if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
            verticalSpeed = JumpVelocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward"); 
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
            _groundVelocity += direction * Accel * deltaF;
        else
            _groundVelocity = _groundVelocity.MoveToward(Vector3.Zero, Decel * deltaF);

        // we actually want diagonals to be faster than cardinals, to mimic build engine movement;
        // so if the player is trying to move in a diagonal direction, give them a speed boost
        var useSpeed = (inputDir.X != 0 && inputDir.Y != 0) ? Speed * BoostSpeedMultiplier : Speed;
        _groundVelocity = _groundVelocity.LimitLength(useSpeed);

        Velocity = _groundVelocity with { Y = verticalSpeed };
        MoveAndSlide();

        PostMove_CameraBob(useSpeed, direction, deltaF);
    }

    private void PostMove_CameraBob(float maxSpeedThisFrame, Vector3 wishDirection, float deltaF)
    {
        // camera bobbing takes into account the *actual* horizontal speed of the player after MoveAndSlide 
        var speedFraction = (Velocity with {Y = 0}).Length() / maxSpeedThisFrame;

        if (IsOnFloor() && wishDirection != Vector3.Zero)
        {
            // scale the bob speed with the player's current speed, so they arent bobbing so much when running directly
            // into a wall
            var bobDelta = deltaF * CameraBobSpeed * speedFraction;
            if (_cameraGoingDown)
            {
                _cameraBobTimer += bobDelta;
                if (_cameraBobTimer > 1f)
                {
                    _cameraBobTimer = Mathf.PingPong(_cameraBobTimer, 1f);
                    _cameraGoingDown = false;
                }
            }
            else
            {
                _cameraBobTimer -= bobDelta;
                if (_cameraBobTimer < 0f)
                {
                    _cameraBobTimer = Mathf.PingPong(_cameraBobTimer, 1f);
                    _cameraGoingDown = true;
                }
            }
        }
        else if (_cameraBobTimer > 0f)
        {
            _cameraBobTimer -= deltaF * CameraBobResetSpeed;
            _cameraGoingDown = true;
            if (_cameraBobTimer < 0f)
                _cameraBobTimer = 0f;
        }
        
        _camera.Position = _cameraStart + Vector3.Zero.Lerp(CameraBob, _cameraBobTimer);
    }
}
