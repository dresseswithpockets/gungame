using Godot;
using System;
using Godot.Collections;
using GunGame;

[Tool]
public partial class MonsterShepard : Node3D
{
    [Export] public Dictionary properties;
    [Export] public Array<InfoShepardNode> shepardNodes;
    [Export] public float runAwayDistance;
    [Export(PropertyHint.Layers3DPhysics)] public uint lineOfSightCollisionMask;
    [Export] public float waitAndHideTime;
    [Export] public float runAwaySpeed;

    private Player _player;
    private Node3D _eyes;
    private float _waitAndHideTimer;
    private bool _running;
    private InfoShepardNode _currentShepardNode;
    private int _currentShepardNodeIndex;
    private Node3D _runAwayNode;

    public void UpdateProperties(Node3D qodotMap)
    {
        runAwayDistance = properties.GetOrDefault("run_distance", 1280);
        runAwayDistance = QodotMath.TbUnitsToGodot(runAwayDistance);

        qodotMap.QodotMapGetAllTargets(Name, properties, ref shepardNodes);
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        _eyes = GetNode<Node3D>("Eyes");
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        ChooseNewSpot();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        if (_player == null)
            return;

        
        // bug: if player walks too quickly at the shepard as its running, and is too close to another node, it might trigger too fast causing weird behaviour
        var canSeePlayer = CanSeePlayer();
        if (canSeePlayer && _player.GlobalPosition.DistanceTo(GlobalPosition) <= runAwayDistance)
            RunAway();
        
        if (_waitAndHideTimer > 0)
        {
            _waitAndHideTimer -= (float)delta;
            if (_waitAndHideTimer < 0)
                RunAway();
        }

        if (_running)
        {
            // we're in the run sequence and should move towards the target run away node
            GlobalPosition = GlobalPosition.MoveToward(_runAwayNode.GlobalPosition, runAwaySpeed * (float)delta);
            if (GlobalPosition.IsEqualApprox(_runAwayNode.GlobalPosition))
            {
                _running = false;
                ChooseNewSpot();
            }
        }
        else if (_waitAndHideTimer <= 0f)
        {
            // we're actively waiting to see the player and for the player to see us
            // 0.5 is within 45 degrees
            var playerLookingAtUs = _player.IsCameraLookingTowards(_eyes.GlobalPosition, 0.5f);
            if (playerLookingAtUs && canSeePlayer)
                _waitAndHideTimer = waitAndHideTime;
        }
    }

    private void RunAway()
    {
        _waitAndHideTimer = 0f;
        _running = true;
        ChooseAppropriateRunAwayNode();
    }

    private bool CanSeePlayer()
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            _eyes.GlobalPosition,
            _player.GetEyeGlobalPosition(),
            lineOfSightCollisionMask);
        
        var result = spaceState.IntersectRay(query);
        if (result.Count == 0)
            return true;

        query.To = _player.GlobalPosition.Lerp(_player.GetEyeGlobalPosition(), 0.5f);
        result = spaceState.IntersectRay(query);
        if (result.Count == 0)
            return true;

        query.To = _player.GlobalPosition;
        result = spaceState.IntersectRay(query);
        if (result.Count == 0)
            return true;
        
        return false;
    }

    private void ChooseAppropriateRunAwayNode()
    {
        var slightUpVector = new Vector3(0f, 0.1f, 0f);
        _currentShepardNode.runAwayNodes.Shuffle();
        foreach (var node in _currentShepardNode.runAwayNodes)
        {
            if (_player.CanSeePosition(node.GlobalPosition + slightUpVector)) continue;
            
            _runAwayNode = node;
            break;
        }
    }

    private void ChooseNewSpot()
    {
        var nextNode = NextNodeInBag();
        if (nextNode == _currentShepardNode)
            nextNode = NextNodeInBag();
        _currentShepardNode = nextNode;
        
        GlobalPosition = _currentShepardNode.GlobalPosition;
    }

    private InfoShepardNode NextNodeInBag()
    {
        _currentShepardNodeIndex++;
        if (_currentShepardNodeIndex >= shepardNodes.Count)
        {
            _currentShepardNodeIndex = 0;
            shepardNodes.Shuffle();
        }

        return shepardNodes[_currentShepardNodeIndex];
    }
}
