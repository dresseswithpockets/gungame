using Godot;
using Godot.Collections;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

[GlobalClass]
public partial class PhysicsSoundCollection : Resource
{
    [Export] public PhysicsSound defaultSound;
    [Export] public Array<PhysicsSound> collection;

    private Dictionary<string, PhysicsSound> _map;

    public PhysicsSound GetSoundByTag(string tag)
    {
        _map ??= new Dictionary<string, PhysicsSound>();
        if (_map.Count != collection.Count + 1)
        {
            _map.Clear();
            foreach (var item in collection)
                _map.Add(item.tagName, item);

            if (defaultSound != null)
                _map["default"] = defaultSound;
        }

        return CollectionExtensions.GetValueOrDefault(_map, tag, defaultSound);
    }
}
