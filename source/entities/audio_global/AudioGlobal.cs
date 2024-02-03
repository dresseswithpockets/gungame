using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class AudioGlobal : AudioStreamPlayer, IAudioPlayer
{
    [Export] public Dictionary properties;
    [Export] public float defaultVolumeDb;
    [Export] public float fadeInTime;
    
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
        
        defaultVolumeDb = properties.GetOrDefault("volume_db", 0f);
        VolumeDb = defaultVolumeDb;
        Autoplay = properties.GetOrDefault("autoplay", false);
        PitchScale = properties.GetOrDefault("pitch_scale", 1f);
        fadeInTime = properties.GetOrDefault("fade_in_time", 0f);
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        
        if (Autoplay && fadeInTime > 0f)
        {
            VolumeDb = -80f;
            FadeInVolume();
        }
    }

    private void FadeInVolume()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "volume_db", defaultVolumeDb, fadeInTime);
        tween.Play();
    }

    private void StartPlaying()
    {
        if (fadeInTime > 0f)
            VolumeDb = -80f;
        else
            VolumeDb = defaultVolumeDb;
        
        Play();

        if (fadeInTime > 0f)
            FadeInVolume();
    }
    
    // ReSharper disable once InconsistentNaming
    public void use(Node3D _)
    {
        if (StreamPaused)
            StreamPaused = false;
        else
            StartPlaying();
    }

    // ReSharper disable once InconsistentNaming
    public void pause(Node3D _) => StreamPaused = true;

    // ReSharper disable once InconsistentNaming
    public void reset(Node3D _) => Stop();
    
    // ReSharper disable once InconsistentNaming
    public void restart(Node3D _) => StartPlaying();
}
