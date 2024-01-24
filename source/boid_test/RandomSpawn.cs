using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RandomSpawn : Node3D
{
    [Export] public int startingBoids = 20;
    [Export] public PackedScene boidPrefab;
    [Export] public Vector3 size;

    public AccelGrid accelGrid;
    public readonly List<Boid> allBoids = new();
    public Vector3I[] scaledPoints;

    private float _targetTime = 5f;
    private float _targetTimer = 0f;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        scaledPoints = new Vector3I[startingBoids];
        InitBoids();
        BuildAccelGrid();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        UpdateBoidFlocks();
        foreach (var boid in allBoids)
            boid.Update((float)delta);

        if (_targetTimer > 0f)
        {
            _targetTimer -= (float)delta;
            if (_targetTimer <= 0f)
            {
                _targetTimer += _targetTime;
                var targetPos = GetRandomPositionInBounds();
                foreach (var boid in allBoids)
                {
                    boid.targets.Clear();
                    boid.targets.Add(targetPos);
                }
            }
        }
    }

    private Vector3 GetRandomPositionInBounds()
    {
        var min = GlobalPosition;
        var max = GlobalPosition + size;
        return new Vector3(
            (float)GD.RandRange(min.X, max.X),
            (float)GD.RandRange(min.Y, max.Y),
            (float)GD.RandRange(min.Z, max.Z));
    }

    private void InitBoids()
    {
        GD.Randomize();
        for (var i = 0; i < startingBoids; i++)
        {
            var initPos = new Vector3(
                (float)GD.RandRange(0, size.X),
                (float)GD.RandRange(0, size.Y),
                (float)GD.RandRange(0, size.Z));
            var boid = boidPrefab.Instantiate<Boid>();
            boid.container = this;
            boid.Position = initPos;
            boid.AddToGroup("boids");
            allBoids.Add(boid);
            AddChild(boid);
        }
    }
    
    private void BuildAccelGrid()
    {
        var structScale = Mathf.FloorToInt(allBoids[0].viewDistance * 0.5f);
        accelGrid = new AccelGrid(size, structScale);
        for (var i = 0; i < allBoids.Count; i++)
        {
            var scaledPoint = accelGrid.ScalePoint(allBoids[i].Position);
            accelGrid.AddBody(scaledPoint, allBoids[i]);
            scaledPoints[i] = scaledPoint;
        }
    }

    private void UpdateBoidFlocks()
    {
        for (var i = 0; i < allBoids.Count; i++)
            allBoids[i].flock = accelGrid.EnumerateNearBodies(scaledPoints[i]).ToList();
    }
}
