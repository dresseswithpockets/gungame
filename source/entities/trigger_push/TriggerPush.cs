using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    [Export] public Vector3 amount;
    [Export] public AxisMask axisMaskFlags;
    [Export] public bool overrideVelocity;

    private void UpdateProperties()
    {
        if (!Engine.IsEditorHint())
            return;

        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;
        
        amount = properties.GetOrDefault("amount", Vector3.Zero);
        axisMaskFlags = (AxisMask)properties.GetOrDefault("axis_mask", (int)AxisMask.Y);
        overrideVelocity = properties.GetOrDefault("override_velocity", false);
    }
    
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var pushable in _overrideVelocityPushables)
        {
            var velocity = pushable.Velocity;
            axisMaskFlags.ApplyMaskedVector(ref velocity, ref amount);
            pushable.OverrideVelocity(velocity);
        }

        foreach (var rigidBody in _overrideVelocityRigidBodies)
        {
            var velocity = rigidBody.LinearVelocity;
            axisMaskFlags.ApplyMaskedVector(ref velocity, ref amount);
            rigidBody.LinearVelocity = velocity;
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        switch (body)
        {
            case RigidBody3D rigidBody when overrideVelocity:
                _overrideVelocityRigidBodies.Add(rigidBody);
                break;
            case RigidBody3D rigidBody:
                rigidBody.AddConstantCentralForce(amount);
                break;
            case IPushable pushable when overrideVelocity:
                _overrideVelocityPushables.Add(pushable);
                break;
            case IPushable pushable:
                pushable.AddContinuousForce(amount);
                break;
        }
        
        Debug.Assert(_overrideVelocityPushables.Distinct().Count() == _overrideVelocityPushables.Count);
        Debug.Assert(_overrideVelocityRigidBodies.Distinct().Count() == _overrideVelocityRigidBodies.Count);
    }

    private void OnBodyExited(Node3D body)
    {
        switch (body)
        {
            case RigidBody3D rigidBody when overrideVelocity:
                _overrideVelocityRigidBodies.Remove(rigidBody);
                break;
            case RigidBody3D rigidBody:
                rigidBody.AddConstantCentralForce(-amount);
                break;
            case IPushable pushable when overrideVelocity:
                _overrideVelocityPushables.Remove(pushable);
                break;
            case IPushable pushable:
                pushable.AddContinuousForce(-amount);
                break;
        }
        
        Debug.Assert(_overrideVelocityPushables.Distinct().Count() == _overrideVelocityPushables.Count);
        Debug.Assert(_overrideVelocityRigidBodies.Distinct().Count() == _overrideVelocityRigidBodies.Count);
    }
}

internal interface IPushable
{
    void AddContinuousForce(Vector3 amount);
    void AddImpulse(Vector3 amount);
    void OverrideVelocity(Vector3 amount);
    Vector3 Velocity { get; }
}