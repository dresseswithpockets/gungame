using Godot;
using System;
using System.Collections.Generic;

public partial class AccelGrid : Node
{
    private List<Boid>[,,] _grid;
    private int _scale;
    public Vector3I size;

    public AccelGrid(Vector3 unscaledSize, int scale)
    {
        _scale = scale;
        size = new Vector3I(ScaleAxis(unscaledSize.X), ScaleAxis(unscaledSize.Y), ScaleAxis(unscaledSize.Z));
        _grid = new List<Boid>[size.X + 1, size.Y + 1, size.X + 1];
        for (var i = 0; i < size.X + 1; i++)
        for (var j = 0; j < size.Y + 1; j++)
        for (var k = 0; k < size.Z + 1; k++)
            _grid[i, j, k] = new List<Boid>();
    }

    private int ScaleAxis(float point) => _scale == 0 ? 0 : Mathf.FloorToInt(point / _scale);

    public Vector3I ScalePoint(Vector3 vector) =>
        new Vector3I(ScaleAxis(vector.X), ScaleAxis(vector.Y), ScaleAxis(vector.Z)).Clamp(Vector3I.Zero, size);

    public IEnumerable<Boid> EnumerateNearBodies(Vector3I scaledPoint)
    {
        scaledPoint = scaledPoint.Clamp(Vector3I.Zero, size);

        var up = scaledPoint.Y + 1;
        var down = scaledPoint.Y - 1;
        var north = scaledPoint.Z - 1;
        var south = scaledPoint.Z + 1;
        var east = scaledPoint.X + 1;
        var west = scaledPoint.X - 1;

        foreach (var body in _grid[scaledPoint.X, scaledPoint.Y, scaledPoint.Z])
            yield return body;

        if (up < size.Y)
        {
            // directly up
            foreach (var body in _grid[scaledPoint.X, up, scaledPoint.Z])
                yield return body;
            
            // up west
            if (west > 0)
            {
                foreach (var body in _grid[west, up, scaledPoint.Z])
                    yield return body;
            }
            
            // up east
            if (east < size.X)
            {
                foreach (var body in _grid[east, up, scaledPoint.Z])
                    yield return body;
            }
            
            // up north
            if (north > 0)
            {
                foreach (var body in _grid[scaledPoint.X, up, north])
                    yield return body;
            }
            
            // up south
            if (south < size.Z)
            {
                foreach (var body in _grid[scaledPoint.X, up, south])
                    yield return body;
            }
        }
        
        
        if (down > 0)
        {
            // directly down
            foreach (var body in _grid[scaledPoint.X, down, scaledPoint.Z])
                yield return body;
            
            // down west
            if (west > 0)
            {
                foreach (var body in _grid[west, down, scaledPoint.Z])
                    yield return body;
            }
            
            // down east
            if (east < size.X)
            {
                foreach (var body in _grid[east, down, scaledPoint.Z])
                    yield return body;
            }
            
            // down north
            if (north > 0)
            {
                foreach (var body in _grid[scaledPoint.X, down, north])
                    yield return body;
            }
            
            // down south
            if (south < size.Z)
            {
                foreach (var body in _grid[scaledPoint.X, down, south])
                    yield return body;
            }
        }

        if (west > 0)
        {
            // directly west
            foreach (var body in _grid[west, scaledPoint.Y, scaledPoint.Z])
                yield return body;
            
            // north west
            if (north > 0)
            {
                foreach (var body in _grid[west, scaledPoint.Y, north])
                    yield return body;
            }
            
            // south west
            if (south > size.Z)
            {
                foreach (var body in _grid[west, scaledPoint.Y, south])
                    yield return body;
            }
        }
    }

    public void AddBody(Vector3I scaledPoint, Boid boid) =>
        _grid[scaledPoint.X, scaledPoint.Y, scaledPoint.Z].Add(boid);
}
