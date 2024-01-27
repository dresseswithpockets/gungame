using System.Diagnostics;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class FuncButton : Area3D, IPlayerUsable
{
    [Export] public Dictionary properties;

    [Signal]
    public delegate void TriggerEventHandler();

    [Export] public float cooldownTime;
    [Export] public float resetDelay;
    [Export] public float resetTime;
    [Export] public float travelTime;
    [Export] public Vector3 targetOffset;
    [Export] public bool disableAfterUse;
    [Export] public AudioStreamPlayer3D audioPlayer;

    private float _cooldownTimer;
    private bool _used;

    public void UpdateProperties(Node3D _)
    {
        if (!Engine.IsEditorHint())
            return;

        if (audioPlayer != null && IsInstanceValid(audioPlayer))
        {
            RemoveChild(audioPlayer);
            audioPlayer.QueueFree();
            audioPlayer = null;
        }

        var angles = properties.GetOrDefault("angles", Vector3.Zero);
        var travelOffset = properties.GetOrDefault("travel_distance", 0f);
        targetOffset = QodotMath.TbAnglesToDirection(angles) * QodotMath.TbUnitsToGodot(travelOffset);
        
        cooldownTime = properties.GetOrDefault("cooldown_time", 0.25f);
        resetDelay = properties.GetOrDefault("reset_delay", 0f);
        resetTime = properties.GetOrDefault("reset_time", 0f);
        travelTime = properties.GetOrDefault("travel_time", 0f);
        disableAfterUse = properties.GetOrDefault("disable_after_use", false);

        if (cooldownTime < 0f)
            GD.PushWarning("cooldown_time must be >= 0, otherwise unexpected behaviour may occur.");
        
        if (resetTime < 0f)
            GD.PushWarning("reset_time must be >= 0, otherwise unexpected behaviour may occur.");
        
        if (travelTime < 0f)
            GD.PushWarning("travel_time must be >= 0, otherwise unexpected behaviour may occur.");

        var soundName = properties.GetOrDefault("sound", "");
        if (string.IsNullOrWhiteSpace(soundName)) return;
        
        if (!soundName.StartsWith("res://"))
            soundName = $"res://{soundName}";
        if (!ResourceLoader.Exists(soundName, nameof(AudioStream)))
        {
            GD.PushWarning(
                $"func_button '{Name}' references a non-existent AudioStream: '{soundName}', it will not play anything");
            return;
        }

        audioPlayer = new AudioStreamPlayer3D();
        audioPlayer.Stream = ResourceLoader.Load<AudioStream>(soundName);
        audioPlayer.VolumeDb = properties.GetOrDefault("sound_volume_db", 0f);
        audioPlayer.PitchScale = properties.GetOrDefault("sound_pitch_scale", 1f);
        AddChild(audioPlayer);
        if (!IsInsideTree()) return;
        var tree = GetTree();
        if (tree == null || !IsInstanceValid(tree)) return;
        var editedSceneRoot = tree.EditedSceneRoot;
        if (editedSceneRoot != null & IsInstanceValid(editedSceneRoot))
            audioPlayer.Owner = editedSceneRoot;
    }

    public async void PlayerUse(Player activator)
    {
        // if this function is called while its still async processing, _used will gate for us
        if (_used) return;
        
        _used = true;
        EmitSignal(SignalName.Trigger, activator);

        if (resetDelay == 0f && travelTime == 0f && resetTime == 0f)
        {
            await WaitForCooldown();
            return;
        }

        if (audioPlayer != null && IsInstanceValid(audioPlayer))
            audioPlayer.Play();
        
        var oldPosition = Position;
        var tween = GetTree().CreateTween();
        tween.TweenCallback(Callable.From(WaitForCooldownCallback));
        tween.TweenProperty(this, "position", oldPosition + targetOffset, travelTime);
        tween.TweenInterval(resetDelay);
        tween.TweenProperty(this, "position", oldPosition, resetTime);
        tween.Play();
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private async Task WaitForCooldown()
    {
        if (cooldownTime == 0f) return;
        await ToSignal(GetTree().CreateTimer(cooldownTime), SceneTreeTimer.SignalName.Timeout);
        _used = false;
    }

    private async void WaitForCooldownCallback()
    {
        await WaitForCooldown();
    }
}