using System.Diagnostics;
using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class Player : CharacterBody3D, IPushable, ITeleportTraveller, IDamageable
{
    [Export] public Dictionary properties;

    public const float MouseYawSpeed = 0.022f;
    public const float MousePitchSpeed = 0.022f;
    public const float MouseSensitivity = 2f;

    [Export(PropertyHint.Layers3DPhysics)] public uint lineOfSightCollisionMask;

    [ExportCategory("Basic Movement")]
    [Export]
    public bool allowMovement;

    [Export(hintString: "suffix:m/s²")] public float runAcceleration = 150f;
    [Export(hintString: "suffix:m/s²")] public float runDeceleration = 50f;
    [Export(hintString: "suffix:m/s")] public float runNormalSpeedCap = 10.0f;
    [Export] public float runBoostSpeedMultiplier = 1.15f;
    [Export(hintString: "suffix:m")] public float maxStepHeight = 0.2f;

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
    [Export]
    public float gravityScale = 1.0f;

    [Export] public float terminalVelocity = 20f;

    [Export(hintString: "suffix:m/s")] public float jumpSpeed = 4.6f;
    [Export(hintString: "suffix:s")] public float jumpBufferTime = 0.3f;

    [ExportCategory("Grappling")]
    [Export]
    public bool allowGrapple;

    [Export] public PackedScene grappleHookPrefab;
    [Export] public Node3D grappleHookStart;
    [Export(hintString: "suffix:m/s²")] public float grappleHookPullAccel = 30f;
    [Export(hintString: "suffix:m/s")] public float grappleHookMaxSpeed = 20f;
    [Export(hintString: "suffix:m/s²")] public float grappleHookMaxSpeedDeceleration = 10f;
    [Export(hintString: "suffix:m/s²")] public float runDecelerationDuringGrappleMomentum = 20f;
    [Export(hintString: "suffix:m/s")] public float grappleHookMinimumSpeed = 10f;
    [Export(hintString: "suffix:s")] public float grappleHookCooldown = 0.3f;

    [ExportCategory("Footsteps")]
    [Export] public Array<AudioStream> footstepClips;

    [Export] public AudioStream jumpClip;
    [Export] public AudioStream jumpLandClip;

    [Export(hintString: "suffix:m")] public float footstepDistance = 1f;

    private Vector3 _defaultRespawnPosition;
    private Vector3 _defaultRespawnRotation;
    private AudioStreamPlayer _deathStreamPlayer;

    private float _footstepDistanceAccumulator;
    private RandomNumberGenerator _rand = new();
    private AudioStreamPlayer3D _footstepStreamPlayer;

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

    private ShapeCast3D _shapeCast;

    // horizontal running velocity without any Y component
    private Vector3 _horizontalRunVelocity;

    private PlayerGrappleHook _grappleHook;

    private Vector3 _continuousForce;
    private Vector3 _impulse;

    private CylinderShape3D _collisionCapsule;

    private bool _alive = true;

    public void UpdateProperties(Node3D _)
    {
        allowMovement = properties.GetOrDefault("start_allow_movement", true);
        allowGrapple = properties.GetOrDefault("start_allow_grapple", true);
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        _defaultRespawnPosition = GlobalPosition;
        _defaultRespawnRotation = GlobalRotation;
        _camera = GetNode<Camera3D>("Camera3D");
        _cameraStart = _camera.Position;
        _collisionCapsule = GetNode<CollisionShape3D>("CollisionShape3D").Shape as CylinderShape3D;
        FloorSnapLength = maxStepHeight;

        _shapeCast = GetNode<ShapeCast3D>("Camera3D/PlayerUseShapeCast");
        _deathStreamPlayer = GetNode<AudioStreamPlayer>("DeathStreamPlayer");
        _footstepStreamPlayer = GetNode<AudioStreamPlayer3D>("FootstepStreamPlayer");
    }

    public override void _Input(InputEvent @event)
    {
        if (Engine.IsEditorHint())
            return;

        if (!_alive || !allowMovement) return;
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        switch (@event)
        {
            case InputEventMouseMotion mouseMotion:
                MoveCamera(mouseMotion.Relative);
                break;
            default:
                if (allowGrapple && @event.IsActionPressed("grapple_hook"))
                    TryFireGrappleHook();
                else if (@event.IsActionPressed("use"))
                    TryFireUse();
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

    private void TryFireUse()
    {
        if (!_shapeCast.IsColliding()) return;

        for (var i = 0; i < _shapeCast.GetCollisionCount(); i++)
        {
            var other = _shapeCast.GetCollider(i);
            // ReSharper disable once InvertIf
            if (other is IPlayerUsable usable)
            {
                usable.PlayerUse(this);
                break;
            }
        }
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
        if (Engine.IsEditorHint()) return;
        if (!_alive) return;

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

        var moveJumpJustPressed = allowMovement && Input.IsActionJustPressed("move_jump");
        var moveJumpPressed = allowMovement && Input.IsActionPressed("move_jump");
        if (moveJumpJustPressed)
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
        else if (moveJumpPressed)
        {
            if (_shouldDoBufferJump)
                StartJump();
        }
        else if (!allowMovement || Input.IsActionJustReleased("move_jump"))
        {
            ResetJumpBuffer();
        }

        _shouldDoBufferJump = false;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        var inputDir = Vector2.Zero;
        if (allowMovement)
            inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        // we actually want diagonals to be faster than cardinals, to mimic build engine movement;
        // so if the player is trying to move in a diagonal direction, give them a speed boost
        var useMaxRunSpeed = (inputDir.X != 0 && inputDir.Y != 0)
            ? runNormalSpeedCap * runBoostSpeedMultiplier
            : runNormalSpeedCap;
        if (_maxSpeedFromGrapple > useMaxRunSpeed)
            useMaxRunSpeed = _maxSpeedFromGrapple;

        var gravity = PhysicsServer3D.BodyGetDirectState(GetRid()).TotalGravity * gravityScale;

        if (_isPulledByGrappleHook)
        {
            var grappleDirection = (_grappleHook!.GlobalPosition - _camera.GlobalPosition).Normalized();
            if (!_wasPulledByGrappleHookLastFrame)
            {
                // we just started grapple-hooking, so transpose the player's current momentum into the direction of
                // the grapple hook, but always ensure at least grappleHookMinimumSpeed speed and at most
                // useMaxRunSpeed speed
                var momentum = _horizontalRunVelocity with { Y = verticalSpeed };
                var newSpeed = Mathf.Clamp(momentum.Length(), grappleHookMinimumSpeed,
                    Mathf.Max(grappleHookMinimumSpeed, useMaxRunSpeed));
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

            // gravity is only applied when not grappling, and when the player isnt beyond the terminal velocity
            if (!IsOnFloor() && verticalSpeed < terminalVelocity)
            {
                verticalSpeed += gravity.Y * deltaF;
            }
        }

        PreMove_JumpSquat(deltaF, ref verticalSpeed);

        var velocityUsedByStepUp = SweepStepUp(deltaF);
        Velocity = _horizontalRunVelocity with { Y = verticalSpeed } - velocityUsedByStepUp;

        var preMoveOnFloor = IsOnFloor();
        MoveAndSlide();

        // if the player lands on a ledge during the rise of the jump, then treat it as if they've completed
        // the jump by the next frame. This ends up behaving pretty much exactly like ion fury's vaulting jump
        if (IsOnFloor() && !preMoveOnFloor)
        {
            _cameraLandingBobTimer = cameraLandingBobTime;
            Velocity = Velocity with { Y = 0f };

            if (_jumpBufferTimer > 0f)
                _shouldDoBufferJump = true;

            EmitJumpLandingSound();
        }

        // TODO: do we still want the landing bob? PostMove_LandingBob(deltaF);
        PostMove_RunBob(deltaF, useMaxRunSpeed, direction);
        _camera.Position = _cameraStart + _cameraAggregateOffset;

        _impulse = Vector3.Zero;

        PostMove_FootstepsSounds(Velocity.Length() * deltaF);
    }

    private void PostMove_FootstepsSounds(float distanceTravelled)
    {
        if (!IsOnFloor() || footstepClips == null || footstepClips.Count == 0 || _footstepStreamPlayer == null ||
            !IsInstanceValid(_footstepStreamPlayer))
            return;

        _footstepDistanceAccumulator += distanceTravelled;
        if (_footstepDistanceAccumulator >= footstepDistance)
        {
            _footstepDistanceAccumulator -= footstepDistance;
            PlayRandomFootstep();
        }
    }

    private void PlayRandomFootstep()
    {
        var clip = footstepClips[_rand.RandiRange(0, footstepClips.Count - 1)];
        _footstepStreamPlayer.Stream = clip;
        _footstepStreamPlayer.Play();
    }

    private Vector3 SweepStepUp(float deltaF)
    {
        if (!IsOnFloor() || _horizontalRunVelocity is { X: 0, Z: 0 })
            return Vector3.Zero;

        var horizontalTravelledByStepping = Vector3.Zero;
        var maxPossibleStepHeight = maxStepHeight;
        var testTransform = GlobalTransform;
        var collisionInfo = new KinematicCollision3D();
        if (TestMove(testTransform, Vector3.Up * maxStepHeight, collisionInfo, SafeMargin))
            maxPossibleStepHeight = collisionInfo.GetTravel().Y;

        var horizontalTravel = _horizontalRunVelocity * deltaF;
        testTransform.Origin.Y += maxPossibleStepHeight;
        if (TestMove(testTransform, horizontalTravel, collisionInfo, SafeMargin))
            horizontalTravel = collisionInfo.GetTravel();

        testTransform.Origin += horizontalTravel;
        if (TestMove(testTransform, Vector3.Down * maxPossibleStepHeight, collisionInfo, SafeMargin) &&
            collisionInfo.GetPosition().Y > GlobalPosition.Y &&
            Vector3.Up.Dot(collisionInfo.GetNormal()) >= 0.7f)
        {
            GlobalPosition = (GlobalPosition + horizontalTravel) with { Y = collisionInfo.GetPosition().Y };
            horizontalTravelledByStepping = horizontalTravel;
        }

        return horizontalTravelledByStepping / deltaF;
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

        EmitJumpingSound();
    }

    private void EmitJumpingSound()
    {
        _footstepStreamPlayer.Stream = jumpClip;
        _footstepStreamPlayer.Play();
    }

    private void EmitJumpLandingSound()
    {
        _footstepStreamPlayer.Stream = jumpLandClip;
        _footstepStreamPlayer.Play();
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

    public void TeleportTo(Node3D targetNode)
    {
        // players origin is at their feet, but props and stuff have an origin at their center, so we can move the
        // player relative to their feet instead. This is the default behaviour.
        // TODO: should the feet-relative stuff be toggleable on the thing calling TeleportTo?
        var targetPos = targetNode.GlobalPosition;
        targetPos.Y -= _collisionCapsule.Height * 0.5f;
        GlobalPosition = targetPos;
        GlobalRotation = GlobalRotation with { Y = GlobalRotation.Y };

        // redirect player's momentum
        var forward = -targetNode.GlobalTransform.Basis.Z;
        _horizontalRunVelocity.Y = Velocity.Y;
        _horizontalRunVelocity = forward * _horizontalRunVelocity.Length();
        Velocity = Velocity with { Y = _horizontalRunVelocity.Y };
        _horizontalRunVelocity.Y = 0f;
    }

    public void Damage(int amount)
    {
        // for now, all damage instantly kills the player
        if (!_alive)
            return;

        Kill();
    }

    private async void Kill()
    {
        _alive = false;
        // TODO: make camera fall to the ground with a canted angle
        // TODO: spawn gibs & blood effects

        _deathStreamPlayer.Play();
        await ToSignal(GetTree().CreateTimer(5), SceneTreeTimer.SignalName.Timeout);
        Respawn();
    }

    private void ResetTimers()
    {
        _jumpBufferTimer = 0f;
        _cameraJumpBobTimer = 0f;
        _cameraLandingBobTimer = 0f;
        _cameraRunBobTimer = 0f;
        _grappleHookCooldownTimer = 0f;
        _cameraRunBobResetTimer = 0f;
    }

    private void ResetVelocity()
    {
        _horizontalRunVelocity = Vector3.Zero;
        Velocity = Vector3.Zero;
    }

    private void Respawn()
    {
        ResetTimers();
        ResetVelocity();
        var respawns = GetTree().GetNodesInGroup("info_player_respawn");
        if (respawns.Count == 0)
        {
            GlobalPosition = _defaultRespawnPosition;
            GlobalRotation = _defaultRespawnRotation;
        }
        else
        {
            var respawn = (Node3D)respawns[_rand.RandiRange(0, respawns.Count - 1)];
            GlobalPosition = respawn.GlobalPosition;
            GlobalRotation = respawn.GlobalRotation;
        }

        _alive = true;
    }

    public bool IsCameraLookingTowards(Vector3 globalPosition, double minFraction)
    {
        var forward = (-_camera.GlobalBasis.Z).Normalized();
        var delta = (globalPosition - _camera.GlobalPosition).Normalized();
        return forward.Dot(delta) >= minFraction;
    }

    public Vector3 GetEyeGlobalPosition() => _camera.GlobalPosition;

    public bool CanSeePosition(Vector3 globalPosition)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            GetEyeGlobalPosition(),
            globalPosition,
            lineOfSightCollisionMask);

        var result = spaceState.IntersectRay(query);
        return result.Count == 0;
    }
}