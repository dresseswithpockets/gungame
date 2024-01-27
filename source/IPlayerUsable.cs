using Godot;

namespace GunGame;

public interface IPlayerUsable
{
    void PlayerUse(Node3D activator);
}

public interface IUsableDoor : IPlayerUsable
{
    void PlayerUse(Node3D activator, bool withSound);
}