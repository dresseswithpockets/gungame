using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class MonsterRailMover : Area3D
{
    [Export] public Dictionary properties;
    [Export] public InfoNode firstNode;
    [Export] public float speed;

    private InfoNode _next;
    private Vector3 _previous;
    private float _edgeDistance;
    private float _edgeTravel;

    public void UpdateProperties(Node3D qodotMap)
    {
        var target = properties.GetOrDefault("target", "");
        if (string.IsNullOrWhiteSpace(target)) return;
        firstNode = GetFirstNode(qodotMap, target);
    }

    private InfoNode GetFirstNode(Node3D qodotMap, string nodeName)
    {
        var nodes = qodotMap.Call("get_nodes_by_targetname", nodeName).AsGodotObjectArray<Node>();
        switch (nodes.Length)
        {
            case 0:
                GD.PushWarning(
                    $"'{Name}' targets '{nodeName}', but there are no entities with that name, so it will not link to anything.");
                return null;
            case > 1:
                GD.PushWarning($"'{Name}' targets multiple entities named '{nodeName}'. Will only link to the first one.");
                break;
        }

        var node = nodes[0];
        if (node is not InfoNode infoNode)
        {
            GD.PushError($"'{Name}' must target a InfoNode-derived entity, but it targets '{nodeName}', which doesn't derive from InfoNode.");
            return null;
        }

        return infoNode;
    }
    
    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        
        _previous = GlobalPosition;
        _next = firstNode;
        if (_next != null)
        {
            var nextDelta = _next.GlobalPosition - _previous;
            _edgeDistance = nextDelta.Length();
            LookAt(GlobalPosition + nextDelta);
        }
        
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        if (_next != null)
        {
            GlobalPosition = _previous.Lerp(_next.GlobalPosition, (_edgeTravel / _edgeDistance));
            LookAt(GlobalPosition + _next.GlobalPosition - _previous);
            _edgeTravel += speed * (float)delta;
            while (_edgeTravel > _edgeDistance)
            {
                _edgeTravel -= _edgeDistance;
                _previous = _next!.GlobalPosition;
                _next = _next.targetNode;
                if (_next == null) break;
                _edgeDistance = (_next.GlobalPosition - _previous).Length();
            }
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is IDamageable damageable)
            damageable.Damage(1);
    }
}
