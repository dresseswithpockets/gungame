using System.Diagnostics;
using Godot;

public partial class Player : CharacterBody3D
{
    public const float Accel = 150f;
    public const float Decel = 50f;
    public const float Speed = 10.0f;
    public const float BoostSpeedMultiplier = 1.15f;
    public const float JumpVelocity = 4.6f;
    public const float YawSpeed = 0.022f;
    public const float PitchSpeed = 0.022f;
    public const float Sensitivity = 2f;
    
    [ExportCategory("Run Bobbing")]
    [Export]
    public Vector3 cameraRunBob = new(0f, -0.3f, 0f);
    [Export] public Curve cameraRunBobCurve;
    [Export] public float cameraRunBobTime = 0.33f;
    [Export] public float cameraRunBobResetTime = 0.1f;

    [ExportCategory("Jump Squatting & Landing")]
    [Export]
    public Vector3 cameraJumpBob = new(0f, -0.15f, 0f);

    [Export] public Curve cameraJumpSquatCurve;
    [Export] public float cameraJumpSquatTime = 0.1f;
    [Export] public bool fullJumpSquatCoyoteTime = true;
    [Export] public Vector3 cameraLandingBob = new(0f, -0.3f, 0f);
    [Export] public Curve cameraLandingBobCurve;
    [Export] public float cameraLandingBobTime = 0.27f;

    [ExportCategory("Falling")]
    [Export]
    public float gravity = 15f;

    private Camera3D _camera;
    private Vector3 _cameraStart;
    private Vector3 _cameraAggregateOffset;
    private float _cameraRunBobTimer;
    private float _cameraRunBobResetTimer;
    private float _cameraJumpBobTimer;
    private float _cameraLandingBobTimer;

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

        // player can only start a jump squat if they havent already started one - _cameraJumpBobTimer is non-zero when
        // jump squatting
        if (IsOnFloor() && _cameraJumpBobTimer == 0f && Input.IsActionJustPressed("move_jump"))
            _cameraJumpBobTimer = cameraJumpSquatTime;

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

        var preMoveOnFloor = IsOnFloor();

        Velocity = _groundVelocity with { Y = verticalSpeed };
        MoveAndSlide();

        if (IsOnFloor() && !preMoveOnFloor)
        {
            _cameraLandingBobTimer = cameraLandingBobTime;
            // if the player lands on a ledge during the rise of the jump, then treat it as if they've completed
            // the jump by the next frame. This ends up behaving pretty much exactly like ion fury's vaulting jump
            Velocity = Velocity with { Y = 0f };
        }

        PostMove_LandingBob(deltaF);
        PostMove_RunBob(deltaF, useSpeed, direction);
        _camera.Position = _cameraStart + _cameraAggregateOffset;
    }

    private void PreMove_JumpSquat(float delta, ref float verticalSpeed)
    {
        if (_cameraJumpBobTimer <= 0f) return;
        
        _cameraJumpBobTimer -= delta;
        if (_cameraJumpBobTimer < 0f)
        {
            _cameraJumpBobTimer = 0f;
            if (fullJumpSquatCoyoteTime || IsOnFloor())
                verticalSpeed = JumpVelocity;
        }

        var alpha = 1f - (_cameraJumpBobTimer / cameraJumpSquatTime);
        var blend = cameraJumpSquatCurve.Sample(alpha);
        _cameraAggregateOffset += Vector3.Zero.Lerp(cameraJumpBob, blend);
    }

    private void PostMove_LandingBob(float delta)
    {
        if (_cameraLandingBobTimer <= 0f) return;

        _cameraLandingBobTimer -= delta;
        if (_cameraLandingBobTimer < 0f)
            _cameraLandingBobTimer = 0f;

        var alpha = 1f - (_cameraLandingBobTimer / cameraLandingBobTime);
        var blend = cameraLandingBobCurve.Sample(alpha);
        _cameraAggregateOffset += Vector3.Zero.Lerp(cameraLandingBob, blend);
    }

    private void PostMove_RunBob(float delta, float maxSpeedThisFrame, Vector3 wishDirection)
    {
        Debug.Assert(maxSpeedThisFrame != 0f);
        
        if (IsOnFloor() && wishDirection != Vector3.Zero)
        {
            if (_cameraRunBobResetTimer > 0f)
            {
                _cameraRunBobResetTimer = 0f;
                _cameraRunBobTimer = cameraRunBobTime * _cameraRunBobResetTimer / cameraRunBobResetTime;
            }
            
            // camera bobbing takes into account the *actual* horizontal speed of the player after MoveAndSlide, so
            // they arent bobbing so much when running directly into a wall.
            var speedFraction = (Velocity with { Y = 0 }).Length() / maxSpeedThisFrame; 
            _cameraRunBobTimer += delta * speedFraction;
            
            // the run bob is looping/wrapping while they're moving
            var t = Mathf.PingPong(_cameraRunBobTimer, cameraRunBobTime);
            
            var alpha = t / cameraRunBobTime;
            var blend = cameraRunBobCurve.Sample(alpha);
            _cameraAggregateOffset += cameraRunBob * blend;
        }
        else if (_cameraRunBobTimer > 0f || _cameraRunBobResetTimer > 0f)
        {
            if (_cameraRunBobTimer > 0f && _cameraRunBobResetTimer == 0f)
            {
                var realRunBobTimer = Mathf.PingPong(_cameraRunBobTimer, cameraRunBobTime);
                _cameraRunBobResetTimer = cameraRunBobResetTime * (realRunBobTimer / cameraRunBobTime);
                _cameraRunBobTimer = 0f;
            }
            
            _cameraRunBobResetTimer -= delta;
            if (_cameraRunBobResetTimer < 0f)
                _cameraRunBobResetTimer = 0f;
            
            var alpha = _cameraRunBobResetTimer / cameraRunBobResetTime;
            var blend = cameraRunBobCurve.Sample(alpha);
            _cameraAggregateOffset += cameraRunBob * blend;
        }
    }
}