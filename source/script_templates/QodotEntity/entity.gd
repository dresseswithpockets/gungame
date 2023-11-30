# meta-name: Qodot Entity
# meta-description: Basic entity with properties
# meta-default: true
# meta-space-indent: 4
@tool
extends QodotEntity

func update_properties():
    if not Engine.is_editor_hint():
        return

#    if you intend to have children node which get created during map build
#
#    for child in get_children():
#        remove_child(child)
#        child.queue_free()

#    if you intend to have children node which get created during map build
#    
#    add_child(my_node)
#    if is_inside_tree():
#        var tree = get_tree()
#        if tree:
#            var edited_scene_root = tree.get_edited_scene_root()
#            if edited_scene_root:
#                my_node.set_owner(edited_scene_root)
