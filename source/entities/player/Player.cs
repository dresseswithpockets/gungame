using System.Diagnostics;
using Godot;

public partial class Player : CharacterBody3D, IPushable
{
    public const float MouseYawSpeed = 0.022f;
    public const float MousePitchSpeed = 0.022f;
    public const float MouseSensitivity = 2f;

    [ExportCategory("Basic Movement")]
    [Export(hintString: "suffix:m/s²")]
    public float runAcceleration = 150f;

    [Export(hintString: "suffix:m/s²")] public float runDeceleration = 50f;
    [Export(hintString: "suffix:m/s")] public float runNormalSpeedCap = 10.0f;
    [Export] public float runBoostSpeedMultiplier = 1.15f;


    [ExportCategory("Run Bobbing")]
    [Export]
    public Vector3 cameraRunBob = new(0f, -0.3f, 0f);

    [Export] public Curve cameraRunBobCurve;
    [Export(hintString: "suffix:s")] public float cameraRunBobTime = 0.33f;
    [Export(hintString: "suffix:s")] public float cameraRunBobResetTime = 0.1f;

    [ExportCategory("Jump Squatting & Landing")]
    [Export]
    public Vector3 cameraJumpBob = new(0f, -0.15f, 0f);

    [Export] public Curve cameraJumpSquatCurve;
    [Export(hintString: "suffix:s")] public float cameraJumpSquatTime = 0.1f;
    [Export(hintString: "suffix:s")] public bool fullJumpSquatCoyoteTime = true;
    [Export] public Vector3 cameraLandingBob = new(0f, -0.3f, 0f);
    [Export] public Curve cameraLandingBobCurve;
    [Export(hintString: "suffix:s")] public float cameraLandingBobTime = 0.27f;

    [ExportCategory("Jumping & Falling")]
    [Export(hintString: "suffix:m/s²")]
    public float gravity = 15f;

    [Export(hintString: "suffix:m/s")] public float jumpSpeed = 4.6f;
    [Export(hintString: "suffix:s")] public float jumpBufferTime = 0.3f;

    [ExportCategory("Grappling")]
    [Export]
    public PackedScene grappleHookPrefab;

    [Export] public Node3D grappleHookStart;
    [Export(hintString: "suffix:m/s²")] public float grappleHookPullAccel = 30f;
    [Export(hintString: "suffix:m/s")] public float grappleHookMaxSpeed = 20f;
    [Export(hintString: "suffix:m/s²")] public float grappleHookMaxSpeedDeceleration = 10f;
    [Export(hintString: "suffix:m/s²")] public float runDecelerationDuringGrappleMomentum = 20f;
    [Export(hintString: "suffix:m/s")] public float grappleHookMinimumSpeed = 10f;
    [Export(hintString: "suffix:s")] public float grappleHookCooldown = 0.3f;
    private float _grappleHookCooldownTimer;
    private bool _isPulledByGrappleHook;
    private bool _wasPulledByGrappleHookLastFrame;
    private float _maxSpeedFromGrapple;

    private float _jumpBufferTimer;
    private bool _shouldDoBufferJump;

    private Camera3D _camera;
    private Vector3 _cameraStart;
    private Vector3 _cameraAggregateOffset;
    private float _cameraRunBobTimer;
    private float _cameraRunBobResetTimer;
    private float _cameraJumpBobTimer;
    private float _cameraLandingBobTimer;

    // horizontal running velocity without any Y component
    private Vector3 _horizontalRunVelocity;

    private PlayerGrappleHook _grappleHook;

    private Vector3 _continuousForce;
    private Vector3 _impulse;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _cameraStart = _camera.Position;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        switch (@event)
        {
            case InputEventMouseMotion mouseMotion:
                MoveCamera(mouseMotion.Relative);
                break;
            default:
                if (@event.IsActionPressed("grapple_hook"))
                    TryFireGrappleHook();
                break;
        }
    }

    public void RemoveGrappleHook()
    {
        _grappleHookCooldownTimer = grappleHookCooldown;
        _grappleHook.QueueFree();
        _grappleHook = null;
        _isPulledByGrappleHook = false;
    }

    private void TryFireGrappleHook()
    {
        if (_grappleHook != null && IsInstanceValid(_grappleHook))
        {
            RemoveGrappleHook();
            return;
        }

        if (_grappleHookCooldownTimer != 0f)
            return;

        _grappleHook = grappleHookPrefab.Instantiate<PlayerGrappleHook>();
        GetParent().AddChild(_grappleHook);
        _grappleHook.GlobalPosition = grappleHookStart.GlobalPosition;
        _grappleHook.GlobalRotation = _camera.GlobalRotation;
        _grappleHook.player = this;
    }

    private void MoveCamera(Vector2 relative)
    {
        RotateY(Mathf.DegToRad(MouseSensitivity * MouseYawSpeed * -relative.X));
        _camera.RotateX(Mathf.DegToRad(MouseSensitivity * MousePitchSpeed * -relative.Y));
        // clamping camera's pitch to +/- 90 to prevent inversion
        var rot = _camera.RotationDegrees;
        rot.X = Mathf.Clamp(rot.X, -90, 90);
        _camera.RotationDegrees = rot;
    }

    public override void _PhysicsProcess(double delta)
    {
        _wasPulledByGrappleHookLastFrame = _isPulledByGrappleHook;
        _isPulledByGrappleHook = _grappleHook != null && IsInstanceValid(_grappleHook) && _grappleHook.IsPulling;

        _cameraAggregateOffset = Vector3.Zero;

        // the vertical part of the player's movement isn't subject to normal run acceleration/deceleration, but
        // is subject to adjustments made by MoveAndSlide, so the vertical part is not maintained in
        // _horizontalRunVelocity
        var verticalSpeed = Velocity.Y;
        var deltaF = (float)delta;

        verticalSpeed += _impulse.Y + (_continuousForce.Y * deltaF);
        _horizontalRunVelocity += _impulse with { Y = 0f } + (_continuousForce with { Y = 0f } * deltaF);

        if (Input.IsActionJustPressed("move_jump"))
        {
            if (_isPulledByGrappleHook)
            {
                var momentum = _horizontalRunVelocity with { Y = verticalSpeed };
                var speed = momentum.Length();
                momentum = momentum.Normalized().Slerp(Vector3.Up, 0.5f) * speed;
                _horizontalRunVelocity = momentum with { Y = 0f };
                verticalSpeed = momentum.Y;
                RemoveGrappleHook();
            }
            else
            {
                if (IsOnFloor())
                    StartJump();
                else
                    BeginJumpBuffer();
            }
        }
        else if (Input.IsActionPressed("move_jump"))
        {
            if (_shouldDoBufferJump)
                StartJump();
        }
        else if (Input.IsActionJustReleased("move_jump"))
        {
            ResetJumpBuffer();
        }

        _shouldDoBufferJump = false;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        // we actually want diagonals to be faster than cardinals, to mimic build engine movement;
        // so if the player is trying to move in a diagonal direction, give them a speed boost
        var useMaxRunSpeed = (inputDir.X != 0 && inputDir.Y != 0)
            ? runNormalSpeedCap * runBoostSpeedMultiplier
            : runNormalSpeedCap;
        if (_maxSpeedFromGrapple > useMaxRunSpeed)
            useMaxRunSpeed = _maxSpeedFromGrapple;

        if (_isPulledByGrappleHook)
        {
            var grappleDirection = (_grappleHook!.GlobalPosition - _camera.GlobalPosition).Normalized();
            if (!_wasPulledByGrappleHookLastFrame)
            {
                // we just started grapple-hooking, so transpose the player's current momentum into the direction of
                // the grapple hook, but always ensure at least grappleHookMinimumSpeed speed and at most
                // useMaxRunSpeed speed
                var momentum = _horizontalRunVelocity with { Y = verticalSpeed };
                var newSpeed = Mathf.Clamp(momentum.Length(), grappleHookMinimumSpeed, useMaxRunSpeed);
                var transposed = grappleDirection * newSpeed;
                _horizontalRunVelocity = transposed with { Y = 0f };
                verticalSpeed = transposed.Y;
            }

            // accelerate player directly towards where they're grappling, up to a max speed
            var accel = grappleDirection * grappleHookPullAccel * deltaF;
            _horizontalRunVelocity += accel with { Y = 0f };
            // since _horizontalRunVelocity.Y is always overwritten by verticalSpeed, we never see the fruits of
            // this acceleration vertically, resulting in a funny bobbing motion and a really slow pull by the
            // grapple. so, add the Y accel explicitly to verticalSpeed instead of to _horizontalRunVelocity.Y
            verticalSpeed += accel.Y;

            _horizontalRunVelocity = _horizontalRunVelocity.LimitLength(grappleHookMaxSpeed);
            _maxSpeedFromGrapple = Mathf.Max(_maxSpeedFromGrapple, _horizontalRunVelocity.Length());

            // also, grappling completely disables gravity
        }
        else
        {
            if (_grappleHookCooldownTimer > 0f)
                _grappleHookCooldownTimer = Mathf.Max(0f, _grappleHookCooldownTimer - deltaF);

            // move normally, and take into account the _maxSpeedFromGrapple achieved during the grapple
            if (direction != Vector3.Zero)
                _horizontalRunVelocity += direction * runAcceleration * deltaF;
            else
            {
                var useDeceleration =
                    _maxSpeedFromGrapple > 0f ? runDecelerationDuringGrappleMomentum : runDeceleration;
                _horizontalRunVelocity = _horizontalRunVelocity.MoveToward(Vector3.Zero, useDeceleration * deltaF);
            }

            // cap max ground speed if we're not being pulled by the grapple hook
            _horizontalRunVelocity = _horizontalRunVelocity.LimitLength(useMaxRunSpeed);
            _maxSpeedFromGrapple = Mathf.MoveToward(_maxSpeedFromGrapple, 0f, grappleHookMaxSpeedDeceleration * deltaF);

            // gravity is only applied when not grappling
            if (!IsOnFloor())
            {
                var totalGravity = PhysicsServer3D.BodyGetDirectState(GetRid()).TotalGravity;
                verticalSpeed += totalGravity.Y * deltaF;
            }
        }

        PreMove_JumpSquat(deltaF, ref verticalSpeed);

        var preMoveOnFloor = IsOnFloor();
        Velocity = _horizontalRunVelocity with { Y = verticalSpeed };
        MoveAndSlide();

        // if the player lands on a ledge during the rise of the jump, then treat it as if they've completed
        // the jump by the next frame. This ends up behaving pretty much exactly like ion fury's vaulting jump
        if (IsOnFloor() && !preMoveOnFloor)
        {
            _cameraLandingBobTimer = cameraLandingBobTime;
            Velocity = Velocity with { Y = 0f };

            if (_jumpBufferTimer > 0f)
                _shouldDoBufferJump = true;
        }

        PostMove_LandingBob(deltaF);
        PostMove_RunBob(deltaF, useMaxRunSpeed, direction);
        _camera.Position = _cameraStart + _cameraAggregateOffset;

        _impulse = Vector3.Zero;
    }

    private void ResetJumpBuffer()
    {
        _jumpBufferTimer = 0f;
    }

    private void BeginJumpBuffer()
    {
        // if the player is in the air, buffer their jump input a bit
        _jumpBufferTimer = jumpBufferTime;
    }

    private void StartJump()
    {
        // player can only start a jump squat if they haven't already started one - _cameraJumpBobTimer is
        // non-zero when jump squatting
        if (_cameraJumpBobTimer == 0f)
            _cameraJumpBobTimer = cameraJumpSquatTime;
    }

    private void PreMove_JumpSquat(float delta, ref float verticalSpeed)
    {
        if (_cameraJumpBobTimer <= 0f) return;

        _cameraJumpBobTimer -= delta;
        if (_cameraJumpBobTimer < 0f)
        {
            _cameraJumpBobTimer = 0f;
            if (fullJumpSquatCoyoteTime || IsOnFloor())
                verticalSpeed = jumpSpeed;
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

    public void AddContinuousForce(Vector3 amount) => _continuousForce += amount;

    public void AddImpulse(Vector3 amount) => _impulse += amount;

    public void OverrideVelocity(Vector3 amount)
    {
        Velocity = Velocity with { Y = amount.Y };
        _horizontalRunVelocity.X = amount.X;
        _horizontalRunVelocity.Z = amount.Z;
    }
}