using System.Diagnostics;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class FuncDoor : AnimatableBody3D, IPlayerUsable
{
    [Export] public Dictionary properties;

    [Export] public float closeAfterTime;
    [Export] public float travelTime;
    [Export] public Vector3 targetOffset;
    [Export] public AudioStreamPlayer3D audioPlayer;
    [Export] public AudioStream openStartSound;
    [Export] public AudioStream openLoopSound;
    [Export] public AudioStream openEndSound;
    [Export] public AudioStream closeStartSound;
    [Export] public AudioStream closeLoopSound;
    [Export] public AudioStream closeEndSound;

    private bool _open;

    public void UpdateProperties(Node3D _)
    {
        if (!Engine.IsEditorHint())
            return;

        SetupAudioPlayers();
        
        var angles = properties.GetOrDefault("angles", Vector3.Zero);
        var travelOffset = properties.GetOrDefault("travel_distance", 0f);
        targetOffset = QodotMath.TbAnglesToDirection(angles) * QodotMath.TbUnitsToGodot(travelOffset);
        
        closeAfterTime = properties.GetOrDefault("close_after_time", 0f);
        travelTime = properties.GetOrDefault("travel_time", 0f);
        
        if (travelTime < 0f)
            GD.PushWarning("travel_time must be >= 0, otherwise unexpected behaviour may occur.");
    }

    private void SetupAudioPlayers()
    {
        if (audioPlayer != null && IsInstanceValid(audioPlayer))
        {
            RemoveChild(audioPlayer);
            audioPlayer.QueueFree();
            audioPlayer = null;
        }
        
        var openStartSoundName = properties.GetOrDefault("open_start_sound", "");
        var openLoopSoundName = properties.GetOrDefault("open_loop_sound", "");
        var openEndSoundName = properties.GetOrDefault("open_end_sound", "");
        var closeStartSoundName = properties.GetOrDefault("close_start_sound", "");
        var closeLoopSoundName = properties.GetOrDefault("close_loop_sound", "");
        var closeEndSoundName = properties.GetOrDefault("close_end_sound", "");
        openStartSound = TryAndWarnSoundLoad(openStartSoundName);
        openLoopSound = TryAndWarnSoundLoad(openLoopSoundName);
        openEndSound = TryAndWarnSoundLoad(openEndSoundName);
        closeStartSound = TryAndWarnSoundLoad(closeStartSoundName);
        closeLoopSound = TryAndWarnSoundLoad(closeLoopSoundName);
        closeEndSound = TryAndWarnSoundLoad(closeEndSoundName);

        audioPlayer = new AudioStreamPlayer3D();
        audioPlayer.Stream = openStartSound;
        audioPlayer.VolumeDb = properties.GetOrDefault("sound_volume_db", 0f);
        audioPlayer.PitchScale = properties.GetOrDefault("sound_pitch_scale", 1f);
        // we need to be able to start the looping sound at the same time as the open start sound 
        if (openLoopSound != null || closeLoopSound != null)
            audioPlayer.MaxPolyphony = 2;

        AddChild(audioPlayer);
        if (!IsInsideTree()) return;
        var tree = GetTree();
        if (tree == null || !IsInstanceValid(tree)) return;
        var editedSceneRoot = tree.EditedSceneRoot;
        if (editedSceneRoot != null & IsInstanceValid(editedSceneRoot))
            audioPlayer.Owner = editedSceneRoot;
    }

    private AudioStream TryAndWarnSoundLoad(string soundName)
    {
        if (string.IsNullOrWhiteSpace(soundName))
            return null;
        
        if (!soundName.StartsWith("res://"))
            soundName = $"res://{soundName}";

        if (!ResourceLoader.Exists(soundName, nameof(AudioStream)))
        {
            GD.PushWarning(
                $"func_door '{Name}' references a non-existent AudioStream: '{soundName}'");
            return null;
        }

        return ResourceLoader.Load<AudioStream>(soundName);
    }

    public async void PlayerUse(Player activator)
    {
        // if this function is called while its still async processing, _used will gate for us
        if (_open) return;
        if (closeAfterTime == 0f && travelTime == 0f) return;
        _open = true;

        PlayAudioStartAndLoop(openStartSound, openLoopSound);
        
        var oldPosition = Position;
        // TODO: this tween can be created at build-time in UpdateProperties
        var tween = GetTree().CreateTween();
        tween.SetProcessMode(Tween.TweenProcessMode.Physics);
        tween.TweenProperty(this, "position", oldPosition + targetOffset, travelTime);
        tween.TweenCallback(Callable.From(() => PlayAudioEnd(openEndSound)));
        if (closeAfterTime > 0f)
        {
            tween.TweenInterval(closeAfterTime);
            tween.TweenCallback(Callable.From(() => PlayAudioStartAndLoop(closeStartSound, closeLoopSound)));
            tween.TweenProperty(this, "position", oldPosition, travelTime);
            tween.TweenCallback(Callable.From(() => PlayAudioEnd(closeEndSound)));
        }

        tween.Play();
        await ToSignal(tween, Tween.SignalName.Finished);
        if (closeAfterTime > 0f)
            _open = false;
    }

    private void PlayAudioStartAndLoop(AudioStream start, AudioStream loop)
    {
        if (audioPlayer == null || !IsInstanceValid(audioPlayer)) return;

        if (start != null)
        {
            audioPlayer.Stream = start;
            audioPlayer.Play();
        }

        if (loop != null)
        {
            audioPlayer.Stream = loop;
            audioPlayer.Play();
        }
    }

    private void PlayAudioEnd(AudioStream end)
    {
        if (audioPlayer == null || !IsInstanceValid(audioPlayer)) return;
        audioPlayer.Stop();
        
        if (end != null)
        {
            audioPlayer.Stream = end;
            audioPlayer.Play();
        }
    }
}