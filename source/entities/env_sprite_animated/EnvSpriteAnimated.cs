using Godot;
using System;
using Godot.Collections;
using GunGame;

[Tool]
public partial class EnvSpriteAnimated : AnimatedSprite3D
{
    [Export] public Dictionary properties;

    public const string NoTexturePath = "res://trenchbroom/textures/no_texture_sprite_frames.tres"; 
    
    public void UpdateProperties(Node3D _)
    {
        Billboard = (BaseMaterial3D.BillboardModeEnum)properties.GetOrDefault("billboard", 0);
        Modulate = properties.GetOrDefault("modulate", Colors.White);
        var pixelSize = properties.GetOrDefault("pixel_size", 1f);
        PixelSize = QodotMath.TbUnitsToGodot(pixelSize);
        Autoplay = "default";
        FlipH = true;
        
        var spritePath = properties.GetOrDefault("sprite_name", "");
        var originalSpritePath = spritePath;
        if (string.IsNullOrWhiteSpace(spritePath))
        {
            GD.PushWarning($"'{Name}' doesn't specify a sprite path");
            SpriteFrames = GD.Load<SpriteFrames>(NoTexturePath);
            return;
        }

        if (!spritePath.StartsWith("res://"))
            spritePath = $"res://{spritePath}";

        if (!spritePath.EndsWith("sprite_frames.tres"))
        {
            if (!spritePath.EndsWith("/"))
                spritePath = $"{spritePath}/";
            spritePath = $"{spritePath}sprite_frames.tres";
        }

        if (!ResourceLoader.Exists(spritePath, "SpriteFrames"))
        {
            GD.PushWarning(
                $"'{Name}' targets a sprite_path '{originalSpritePath}', but there is no SpriteFrames resource at '{spritePath}'.");
            SpriteFrames = GD.Load<SpriteFrames>(NoTexturePath);
            return;
        }

        SpriteFrames = GD.Load<SpriteFrames>(spritePath);
    }
}
