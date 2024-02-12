using Godot;
using Godot.Collections;

[Tool]
public partial class FoliageTree : Node3D
{
    [Export] public Dictionary properties;
    [Export] public Array<Material> cardMaterials;
    [Export] public Array<Sprite3D> sprites;

    public void UpdateProperties(Node3D _)
    {
        var material = cardMaterials.PickRandom();
        foreach (var sprite in sprites)
            sprite.MaterialOverride = material;
    }
}
