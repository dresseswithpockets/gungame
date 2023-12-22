using System.Diagnostics;
using Godot;

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
    public static readonly Vector3 CameraRunBob = new(0f, -0.15f, 0f);
    public const float CameraRunBobSpeed = 3f;
    public const float CameraRunBobResetSpeed = 10f;
    public static readonly Vector3 CameraJumpBob = new(0f, -0.1f, 0f);
    public const float CameraJumpBobSpeed = 15f;
    public const bool FullJumpSquatCoyoteTime = true;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private Camera3D _camera;
    private Vector3 _cameraStart;
    private Vector3 _cameraAggregateOffset;
    private float _cameraRunBobTimer;
    private bool _cameraRunBobGoingDown = true;
    private float _cameraJumpBobTimer;
    private bool _cameraJumpBobGoingDown = true;

    private bool _jumpSquatting;
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
        _cameraAggregateOffset = Vector3.Zero;
        
        var verticalSpeed = Velocity.Y;
        var deltaF = (float)delta;

        // Add the gravity.
        if (!IsOnFloor())
            verticalSpeed -= gravity * deltaF;

        // player can only start a jump squat if they havent already started one
        if (IsOnFloor() && !_jumpSquatting && Input.IsActionJustPressed("move_jump"))
            _jumpSquatting = true;

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

        PreMove_JumpSquat(deltaF, ref verticalSpeed);

        Velocity = _groundVelocity with { Y = verticalSpeed };
        MoveAndSlide();

        PostMove_RunBob(deltaF, useSpeed, direction);
        _camera.Position = _cameraStart + _cameraAggregateOffset;
    }

    private void PreMove_JumpSquat(float delta, ref float verticalSpeed)
    {
        if (_jumpSquatting)
        {
            var bobDelta = delta * CameraJumpBobSpeed;
            if (_cameraJumpBobGoingDown)
            {
                _cameraJumpBobTimer += bobDelta;
                if (_cameraJumpBobTimer > 1f)
                {
                    _cameraJumpBobTimer = Mathf.PingPong(_cameraJumpBobTimer, 1f);
                    _cameraJumpBobGoingDown = false;
                }
            }
            else
            {
                _cameraJumpBobTimer -= bobDelta;
                if (_cameraJumpBobTimer < 0f)
                {
                    _cameraJumpBobTimer = 0f;
                    _cameraJumpBobGoingDown = true;
                    _jumpSquatting = false;
                    if (FullJumpSquatCoyoteTime || IsOnFloor())
                        verticalSpeed = JumpVelocity;
                }
            }
        }

        _cameraAggregateOffset += Vector3.Zero.Lerp(CameraJumpBob, _cameraJumpBobTimer);
    }

    private void PostMove_RunBob(float delta, float maxSpeedThisFrame, Vector3 wishDirection)
    {
        Debug.Assert(maxSpeedThisFrame != 0f);
        
        // camera bobbing takes into account the *actual* horizontal speed of the player after MoveAndSlide 
        var speedFraction = (Velocity with {Y = 0}).Length() / maxSpeedThisFrame;

        if (IsOnFloor() && wishDirection != Vector3.Zero)
        {
            // scale the bob speed with the player's current speed, so they arent bobbing so much when running directly
            // into a wall
            var bobDelta = delta * CameraRunBobSpeed * speedFraction;
            if (_cameraRunBobGoingDown)
            {
                _cameraRunBobTimer += bobDelta;
                if (_cameraRunBobTimer > 1f)
                {
                    _cameraRunBobTimer = Mathf.PingPong(_cameraRunBobTimer, 1f);
                    _cameraRunBobGoingDown = false;
                }
            }
            else
            {
                _cameraRunBobTimer -= bobDelta;
                if (_cameraRunBobTimer < 0f)
                {
                    _cameraRunBobTimer = Mathf.PingPong(_cameraRunBobTimer, 1f);
                    _cameraRunBobGoingDown = true;
                }
            }
        }
        else if (_cameraRunBobTimer > 0f)
        {
            _cameraRunBobTimer -= delta * CameraRunBobResetSpeed;
            _cameraRunBobGoingDown = true;
            if (_cameraRunBobTimer < 0f)
                _cameraRunBobTimer = 0f;
        }
        
        _cameraAggregateOffset += Vector3.Zero.Lerp(CameraRunBob, _cameraRunBobTimer);
    }
}
