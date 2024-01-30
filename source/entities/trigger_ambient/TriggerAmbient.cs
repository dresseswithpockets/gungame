using System.Diagnostics;
using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class TriggerAmbient : Area3D
{
    [Export] public Dictionary properties;
    [Export] public Node targetFadeFrom;
    [Export] public Node targetFadeTo;
    [Export] public float fadeTime;
    [Export] public bool stopAfterFade;

    private AudioController _globalAudioController;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;
        
        CollisionMask = (uint)properties.GetOrDefault("collision_mask", 0);
        CollisionLayer = 0;

        stopAfterFade = properties.GetOrDefault("stop_after_fade", false);
        fadeTime = properties.GetOrDefault("fade_time", 1.0f);
        var targetFadeFromName = properties.GetOrDefault("target", "");
        var targetFadeToName = properties.GetOrDefault("target1", "");
        if (string.IsNullOrWhiteSpace(targetFadeFromName) && string.IsNullOrWhiteSpace(targetFadeToName))
        {
            GD.PushWarning($"'{Name}' has no targets to fade from or to, so it won't do anything.");
            return;
        }
        
        targetFadeFrom = GetFirstNode(qodotMap, targetFadeFromName);
        targetFadeTo = GetFirstNode(qodotMap, targetFadeToName);
    }

    private Node GetFirstNode(Node3D qodotMap, string nodeName)
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

        var targetNode = nodes[0];
        if (targetNode == this)
        {
            GD.PushError($"'{Name}' targets itself, but must target a different entity.");
            return null;
        }

        if (targetNode is not AudioStreamPlayer and not AudioStreamPlayer3D)
        {
            GD.PushError($"'{Name}' must target a Audio*-derived entity, but it targets '{nodeName}', which doesn't derive from AudioGlobal or AudioPositional.");
            return null;
        }

        return targetNode;
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        _globalAudioController = GetNode<AudioController>("/root/AudioControllerGlobal");
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        GD.Print(body.Name);
        Debug.Assert(targetFadeFrom is null or AudioGlobal or AudioPositional);
        Debug.Assert(targetFadeTo is null or AudioGlobal or AudioPositional);
        _globalAudioController.QueueFade(targetFadeFrom, targetFadeTo, fadeTime, stopAfterFade);
    }
}
