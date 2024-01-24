using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Boid : Node3D
{
    [Export] public float maxSpeed = 1f;
    [Export] public float minSpeed = 0.8f;
    [Export] public float targetForce = 2f;
    [Export] public float cohesion = 2f;
    [Export] public float alignment = 3f;
    [Export] public float separation = 5f;
    [Export] public float viewDistance = 0.5f;
    [Export] public float avoidDistance = 0.2f;
    [Export] public int maxFlockSize = 15;
    [Export] public float edgeAvoidForce = 10f;

    public RandomSpawn container;
    public readonly List<Vector3> targets = new();
    public Vector3 velocity;
    public bool avoidBoundingEdge = true;

    public List<Boid> flock;
    public int flockSize = 0;

    private Vector3 _flockCenterVector;
    private Vector3 _flockAlignVector;
    private Vector3 _flockAvoidVector;
    private int _flockOtherCount = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Randomize();
        velocity = new Vector3(GD.RandRange(-1, 1), (float)GD.RandRange(-0.5, 0.5), GD.RandRange(-1, 1)).Normalized();
        velocity *= maxSpeed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void Update(float delta)
    {
        Position += velocity * delta;

        var edgeAvoidVector = Vector3.Zero;
        if (avoidBoundingEdge)
            edgeAvoidVector = CalculateAvoidScreenEdge() * edgeAvoidForce;
        else
            WrapEdges();

        CalculateFlockVectors();

        var cohesionVector = _flockCenterVector * cohesion;
        var alignVector = _flockAlignVector * alignment;
        var separationVector = _flockAvoidVector * separation;

        var acceleration = alignVector + cohesionVector + separationVector + edgeAvoidVector;
        if (targets.Count > 0)
        {
            var targetVector = Vector3.Zero;
            foreach (var target in targets)
                targetVector += GlobalPosition.DirectionTo(target);
            targetVector /= targets.Count;
            acceleration += targetVector * targetForce;
        }

        velocity = (velocity + acceleration).LimitLength(maxSpeed);
        if (velocity.LengthSquared() <= minSpeed * minSpeed)
            velocity = (velocity * minSpeed).LimitLength(maxSpeed);

        LookAt(GlobalPosition + velocity);
    }

    private Vector3 CalculateAvoidScreenEdge()
    {
        var bounds = container.size;
        var edgeAvoidVector = Vector3.Zero;
        if (Position.X - avoidDistance < 0)
            edgeAvoidVector.X = 1;
        else if (Position.X + avoidDistance > bounds.X)
            edgeAvoidVector.X = -1;

        if (Position.Y - avoidDistance < 0)
            edgeAvoidVector.Y = 1;
        else if (Position.Y + avoidDistance > bounds.Y)
            edgeAvoidVector.Y = -1;
        
        if (Position.Z - avoidDistance < 0)
            edgeAvoidVector.Z = 1;
        else if (Position.Z + avoidDistance > bounds.Z)
            edgeAvoidVector.Z = -1;

        return edgeAvoidVector.Normalized();
    }

    private void WrapEdges()
    {
        var bounds = container.size;
        Position = new Vector3(
            Mathf.Wrap(Position.X, 0, bounds.X),
            Mathf.Wrap(Position.Y, 0, bounds.Y),
            Mathf.Wrap(Position.Z, 0, bounds.Z));
    }

    private void CalculateFlockVectors()
    {
        var flockCenter = Vector3.Zero;
        _flockCenterVector = Vector3.Zero;
        _flockAlignVector = Vector3.Zero;
        _flockAvoidVector = Vector3.Zero;
        _flockOtherCount = 0;
        
        foreach (var other in flock)
        {
            if (_flockOtherCount == maxFlockSize)
                break;

            if (other == this)
                continue;

            var otherPos = other.GlobalPosition;
            var otherVelocity = other.velocity;

            var distanceSquared = GlobalPosition.DistanceSquaredTo(otherPos);
            if (distanceSquared < viewDistance * viewDistance)
            {
                _flockOtherCount++;
                _flockAlignVector += otherVelocity;
                flockCenter += otherPos;

                if (distanceSquared < avoidDistance * avoidDistance)
                    _flockAvoidVector -= otherPos - GlobalPosition;
            }
        }

        if (_flockOtherCount <= 0) return;
        _flockAlignVector /= _flockOtherCount;
        flockCenter /= _flockOtherCount;
        _flockCenterVector = GlobalPosition.DirectionTo(flockCenter);
    }
}