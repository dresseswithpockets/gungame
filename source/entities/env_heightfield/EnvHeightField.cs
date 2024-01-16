using Godot;
using Godot.Collections;
using GunGame;

[Tool]
public partial class EnvHeightField : Node3D
{
    [Export] public Dictionary properties;

    public void UpdateProperties(Node3D qodotMap)
    {
        if (!Engine.IsEditorHint())
            return;

        var size = Vector3.Zero;
        var heightField = this.FindFirstImmediateChild<GpuParticlesCollisionHeightField3D>();
        if (heightField != null)
        {
            size = heightField.Size;
            RemoveChild(heightField);
            heightField.QueueFree();
        }

        var meshInstance = this.FindFirstImmediateChild<MeshInstance3D>();
        if (meshInstance != null && IsInstanceValid(meshInstance))
        {
            //Position = meshInstance.GetAabb().Position;
            size = meshInstance.GetAabb().Size;
            RemoveChild(meshInstance);
            meshInstance.QueueFree();
        }

        heightField = new GpuParticlesCollisionHeightField3D();
        heightField.Size = size;
        AddChild(heightField);
        if (!IsInsideTree()) return;
        
        var tree = GetTree();
        if (tree == null || !IsInstanceValid(tree)) return;
        
        var editedSceneRoot = tree.EditedSceneRoot;
        if (editedSceneRoot != null && IsInstanceValid(editedSceneRoot))
            heightField.Owner = editedSceneRoot;
    }
}
