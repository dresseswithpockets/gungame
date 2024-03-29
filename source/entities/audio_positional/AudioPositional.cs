using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class AudioPositional : AudioStreamPlayer3D, IAudioPlayer
{
    [Export] public Dictionary properties;
    [Export] public float defaultVolumeDb;
    
    public float DefaultVolumeDb => defaultVolumeDb;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        var soundName = properties.GetOrDefault("sound", "");
        if (!soundName.StartsWith("res://"))
            soundName = $"res://{soundName}";
        if (!ResourceLoader.Exists(soundName, nameof(AudioStream)))
        {
            GD.PushWarning($"'{Name}' references a non-existent AudioStream: '{soundName}', it will not play anything");
            return;
        }
        
        Stream = ResourceLoader.Load<AudioStream>(soundName);
        
        var startSilent = properties.GetOrDefault("start_silent", false);
        defaultVolumeDb = properties.GetOrDefault("volume_db", 0f);
        VolumeDb = startSilent ? -80f : defaultVolumeDb;
        Autoplay = properties.GetOrDefault("autoplay", false);
        PitchScale = properties.GetOrDefault("pitch_scale", 1f);
        MaxDistance = properties.GetOrDefault("max_distance", 0f);
    }
    
    // ReSharper disable once InconsistentNaming
    public void use(Node3D _)
    {
        if (StreamPaused)
            StreamPaused = false;
        else
            Play();
    }

    // ReSharper disable once InconsistentNaming
    public void pause(Node3D _) => StreamPaused = true;

    // ReSharper disable once InconsistentNaming
    public void reset(Node3D _) => Stop();
    
    // ReSharper disable once InconsistentNaming
    public void restart(Node3D _) => Play();
}
