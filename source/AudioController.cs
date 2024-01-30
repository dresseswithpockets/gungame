using System.Collections.Generic;
using Godot;

namespace GunGame;

public partial class AudioController : Node
{
    private Tween _currentFade;
    private readonly Queue<(Node, Node, float, bool)> _audioFadeQueue = new();
    
    public override void _Process(double delta)
    {
        if (_audioFadeQueue.Count <= 0 || _currentFade != null) return;
        
        var (from, to, time, stopAfter) = _audioFadeQueue.Dequeue();
        ProcessAudioFade(from, to, time, stopAfter);
    }

    public void QueueFade(Node targetFadeFrom, Node targetFadeTo, float fadeTime, bool stopAfterFade)
    {
        if (_currentFade != null)
        {
            _audioFadeQueue.Enqueue((targetFadeFrom, targetFadeTo, fadeTime, stopAfterFade));
            return;
        }

        ProcessAudioFade(targetFadeFrom, targetFadeTo, fadeTime, stopAfterFade);
    }

    private async void ProcessAudioFade(Node targetFadeFrom, Node targetFadeTo, float fadeTime, bool stopAfterFade)
    {
        _currentFade = CreateAudioFadeTween(targetFadeFrom, targetFadeTo, fadeTime, stopAfterFade);
        _currentFade.Play();
        await ToSignal(_currentFade, Tween.SignalName.Finished);
        _currentFade = null;
    }

    private Tween CreateAudioFadeTween(Node targetFadeFrom, Node targetFadeTo, float fadeTime, bool stopAfterFade)
    {
        var tween = GetTree().CreateTween();
        if (targetFadeFrom != null)
        {
            var startingVolume = targetFadeFrom is IAudioPlayer audioPlayer ? audioPlayer.DefaultVolumeDb : 0f;
            EnsureIsPlayingAt(targetFadeFrom, startingVolume);
            
            tween.TweenProperty(targetFadeFrom, "volume_db", -80f, fadeTime);
        }

        if (targetFadeTo != null)
        {
            EnsureIsPlayingAt(targetFadeTo, -80f);

            if (targetFadeFrom != null)
                tween.Parallel();

            var targetVolume = targetFadeTo is IAudioPlayer audioPlayer ? audioPlayer.DefaultVolumeDb : 0f;
            tween.TweenProperty(targetFadeTo, "volume_db", targetVolume, fadeTime);
        }

        if (stopAfterFade && targetFadeFrom != null)
            tween.TweenCallback(Callable.From(() => targetFadeFrom.Call("stop")));
        return tween;
    }

    private static void EnsureIsPlayingAt(Node targetFadeTo, float volumeDb)
    {
        switch (targetFadeTo)
        {
            case AudioStreamPlayer audioStreamPlayer:
            {
                if (!audioStreamPlayer.Playing)
                {
                    audioStreamPlayer.VolumeDb = volumeDb;
                    audioStreamPlayer.Play();
                }
                break;
            }
            case AudioStreamPlayer3D audioStreamPlayer3D:
            {
                if (!audioStreamPlayer3D.Playing)
                {
                    audioStreamPlayer3D.VolumeDb = volumeDb;
                    audioStreamPlayer3D.Play();
                }
                break;
            }
        }
    }
}