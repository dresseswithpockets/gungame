using Godot;
using System;

[GlobalClass]
public partial class PhysicsSound : Resource
{
    [Export] public StringName tagName;
    [Export] public AudioStream footstepSoundStream;
}
