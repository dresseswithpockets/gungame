using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class AudioPositional : AudioStreamPlayer3D
{
    private Dictionary _properties;
    // the gdscript side expects lowercase properties :(
    // ReSharper disable once InconsistentNaming
    [Export]
    public Dictionary properties
    {
        get => _properties;
        set
        {
            if (_properties == value) return;
            _properties = value;
            UpdateProperties();
        }
    }

    private void UpdateProperties()
    {
        if (!Engine.IsEditorHint())
            return;

        var soundName = properties.GetOrDefault("sound", "");
        if (!soundName.StartsWith("res://"))
            soundName = $"res://{soundName}";
        if (!ResourceLoader.Exists(soundName, nameof(AudioStream)))
        {
            // TODO: warn non-existent sound
            return;
        }
        
        Stream = ResourceLoader.Load<AudioStream>(soundName);
        VolumeDb = properties.GetOrDefault("volume_db", 0f);
        Autoplay = properties.GetOrDefault("autoplay", false);
        PitchScale = properties.GetOrDefault("pitch_scale", 1f);
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
