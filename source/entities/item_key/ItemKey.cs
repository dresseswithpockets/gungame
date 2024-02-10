using System;
using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class ItemKey : Area3D
{
    [Export] public Dictionary properties;
    
    [Export] public ItemKeyColor color;
    [Export] public float bobAmplitude = 0.25f;
    [Export] public float bobSpeed = 1f;
    [Export] public float rotateSpeed = 90f;
    [Export] public float pitchMin = 0.8f;
    [Export] public float pitchMax = 1f;
    [Export] public Material blackMaterial;
    [Export] public Material purpleMaterial;
    [Export] public Material orangeMaterial;
    [Export] public Material redMaterial;

    private PlayerInventory _playerInventory;
    private AudioStreamPlayer3D _audioStreamPlayer3D;
    private MeshInstance3D _render;
    private Node3D _renderRoot;
    private bool _pickingUp;

    public void UpdateProperties(Node3D _)
    {
        color = (ItemKeyColor)properties.GetOrDefault("key", 0);
    }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        
        _playerInventory = GetNode<PlayerInventory>("/root/PlayerInventory");
        _audioStreamPlayer3D = GetNode<AudioStreamPlayer3D>("PickupSoundPlayer");
        _render = GetNode<MeshInstance3D>("RenderRoot/Render");
        _renderRoot = GetNode<Node3D>("RenderRoot");

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        var material = color switch
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        {
            ItemKeyColor.Black => blackMaterial,
            ItemKeyColor.Purple => purpleMaterial,
            ItemKeyColor.Orange => orangeMaterial,
            ItemKeyColor.Red => redMaterial,
        };

        _render.MaterialOverride = material;
        for (var i = 0; i < _render.Mesh.GetSurfaceCount(); i++)
            _render.SetSurfaceOverrideMaterial(i, material);
        
        BodyEntered += OnBodyEntered;
    }

    private float _bobTimer;

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;
        
        _bobTimer += (float)delta;
        _renderRoot.RotateY(Mathf.DegToRad(rotateSpeed * (float)delta));
        _renderRoot.Position = _renderRoot.Position with
        {
            Y = bobAmplitude * Mathf.Sin(bobSpeed * _bobTimer)
        };
    }

    private async void OnBodyEntered(Node3D body)
    {
        if (_pickingUp) return;
        _pickingUp = true;
        
        _playerInventory.AddKey(color);
        _render.Visible = false;
        _audioStreamPlayer3D.PitchScale = Mathf.Lerp(pitchMin, pitchMax, (float)(int)color / (int)ItemKeyColor.Max);
        _audioStreamPlayer3D.Play();
        await ToSignal(_audioStreamPlayer3D, AudioStreamPlayer3D.SignalName.Finished);
        QueueFree();
    }
}

public enum ItemKeyColor
{
    Black = 0,
    Purple = 1,
    Orange = 2,
    Red = 3,

    Max = 4,
}
