using Godot;
using System;
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

    private PlayerInventory _playerInventory;
    private AudioStreamPlayer3D _audioStreamPlayer3D;
    private Node3D _render;
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
        _render = GetNode<Node3D>("Render");
        BodyEntered += OnBodyEntered;
    }

    private float _bobTimer;

    public override void _Process(double delta)
    {
        _bobTimer += (float)delta;
        _render.RotateY(Mathf.DegToRad(rotateSpeed * (float)delta));
        _render.Position = _render.Position with
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
        _audioStreamPlayer3D.Play();
        await ToSignal(_audioStreamPlayer3D, AudioStreamPlayer3D.SignalName.Finished);
        QueueFree();
    }
}

public enum ItemKeyColor
{
    Black,
    Purple,
    Orange,
    Red,
}
