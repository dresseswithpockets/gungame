@tool
class_name EnvSprite extends QodotEntity

var _no_texture: Texture2D = preload("res://trenchbroom/textures/no_texture_8.png")

# properties:
#  sprite_path: string path to Texture resource
#  billboard: Choices [ FaceCamera, YBillboard ]
#  modulate: Color

enum { BILLBOARD_DISABLED = 0, BILLBOARD_ENABLED = 1, BILLBOARD_Y = 2 }

func update_properties():
    if not Engine.is_editor_hint():
        return

    for child in get_children():
        remove_child(child)
        child.queue_free()
    
    var texture: Texture2D
    var sprite_path = properties.get("sprite_path")
    if sprite_path is String:
        if !sprite_path.begins_with("res://"):
            sprite_path = "res://" + sprite_path
        
        if ResourceLoader.exists(sprite_path, "Texture2D"):
            texture = load(sprite_path)
        else:
            printerr("env_sprite: No Texture2D at sprite_path: '%s'" % sprite_path)
            texture = _no_texture
    else:
        printerr("env_sprite: No sprite_path specified, or the value is invalid")
        texture = _no_texture
    
    var billboard: int = properties.get("billboard", BILLBOARD_DISABLED)
    
    var sprite_node := Sprite3D.new()
    sprite_node.texture = texture
    sprite_node.modulate = properties.get("modulate", Color.WHITE)
    sprite_node.billboard = billboard
    
    add_child(sprite_node)
    
    if is_inside_tree():
        var tree = get_tree()
        if tree:
            var edited_scene_root = tree.get_edited_scene_root()
            if edited_scene_root:
                sprite_node.set_owner(edited_scene_root)
