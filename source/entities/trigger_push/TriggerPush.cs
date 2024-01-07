using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using GunGame;

[Tool]
public partial class TriggerPush : Area3D
{
    private Dictionary _properties;
    // the gdscript side expects lowercase properties :(
    // ReSharper disable once InconsistentNaming
    [Export]
    public Dictionary properties
    {
        get => _properties;
        set
        {
            if (_properties == value) return;
            _properties = value;
            UpdateProperties();
        }
    }

    private readonly List<IPushable> _overrideVelocityPushables = new();
    private readonly List<RigidBody3D> _overrideVelocityRigidBodies = new();

    private void UpdateProperties()
    {
        if (!Engine.IsEditorHint())
            return;

        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;
    }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (_overrideVelocityPushables.Count == 0 && _overrideVelocityRigidBodies.Count == 0)
            return;
        
        var amount = properties.GetOrDefault("amount", Vector3.Zero);
        foreach (var pushable in _overrideVelocityPushables)
            pushable.OverrideVelocity(amount, (AxisMask)properties.GetOrDefault("axis_mask", 0));

        foreach (var rigidBody in _overrideVelocityRigidBodies)
            rigidBody.LinearVelocity = amount;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is RigidBody3D rigidBody)
        {
            var mode = (PushMode)properties.GetOrDefault("mode", (int)PushMode.Continuous);
            var amount = properties.GetOrDefault("amount", Vector3.Zero);
            switch (mode)
            {
                case PushMode.Continuous:
                    rigidBody.AddConstantCentralForce(amount);
                    break;
                case PushMode.Impulse:
                    rigidBody.ApplyCentralImpulse(amount);
                    break;
                case PushMode.OverrideVelocity:
                    _overrideVelocityRigidBodies.Add(rigidBody);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (body is IPushable pushable)
        {
            var mode = (PushMode)properties.GetOrDefault("mode", (int)PushMode.Continuous);
            var amount = properties.GetOrDefault("amount", Vector3.Zero);
            switch (mode)
            {
                case PushMode.Continuous:
                    pushable.AddContinuousForce(amount);
                    break;
                case PushMode.Impulse:
                    pushable.AddImpulse(amount);
                    break;
                case PushMode.OverrideVelocity:
                    _overrideVelocityPushables.Add(pushable);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnBodyExited(Node3D body)
    {
        switch (body)
        {
            case RigidBody3D rigidBody:
            {
                var mode = (PushMode)properties.GetOrDefault("mode", (int)PushMode.Continuous);
                var amount = properties.GetOrDefault("amount", Vector3.Zero);
                switch (mode)
                {
                    case PushMode.Continuous:
                        rigidBody.AddConstantCentralForce(-amount);
                        break;
                    case PushMode.Impulse: // impulse is only applied on Enter once, no need to reverse it on Exit
                        break;
                    case PushMode.OverrideVelocity:
                        _overrideVelocityRigidBodies.Remove(rigidBody);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            }
            case IPushable pushable:
            {
                var mode = (PushMode)properties.GetOrDefault("mode", (int)PushMode.Continuous);
                var amount = properties.GetOrDefault("amount", Vector3.Zero);
                switch (mode)
                {
                    case PushMode.Continuous:
                        pushable.AddContinuousForce(-amount);
                        break;
                    case PushMode.Impulse:
                        break;
                    case PushMode.OverrideVelocity:
                        _overrideVelocityPushables.Remove(pushable);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            }
        }
    }

    enum PushMode
    {
        Continuous,
        Impulse,
        OverrideVelocity,
    }
}

internal interface IPushable
{
    void AddContinuousForce(Vector3 amount);
    void AddImpulse(Vector3 amount);
    void OverrideVelocity(Vector3 amount, AxisMask axisMask);
}