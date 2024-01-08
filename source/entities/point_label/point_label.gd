@tool
extends QodotEntity

func update_properties():
    if not Engine.is_editor_hint():
        return

    for child in get_children():
        remove_child(child)
        child.queue_free()

    # label flags
    var billboard = properties.get("billboard", 0)
    var shaded = properties.get("shaded", false)
    var double_sided = properties.get("double_sided", true)
    var no_depth_test = properties.get("no_depth_test", false)
    var fixed_size = properties.get("fixed_size", false)
    
    # text properties
    var modulate = properties.get("color", Color.WHITE)
    var outline_modulate = properties.get("outline_color", Color.BLACK)
    var text = properties.get("text", "")
    # TODO: font and other text settings

    var label := Label3D.new()
    label.billboard = billboard
    label.shaded = shaded
    label.double_sided = double_sided
    label.no_depth_test = no_depth_test
    label.fixed_size = fixed_size
    label.modulate = modulate
    label.outline_modulate = outline_modulate
    label.text = text
    label.texture_filter = BaseMaterial3D.TEXTURE_FILTER_NEAREST
    label.alpha_cut = Label3D.ALPHA_CUT_DISCARD

    add_child(label)
    if is_inside_tree():
        var tree = get_tree()
        if tree:
            var edited_scene_root = tree.get_edited_scene_root()
            if edited_scene_root:
                label.set_owner(edited_scene_root)
