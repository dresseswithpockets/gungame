@tool
extends QodotEntity

const default_radius: float = 5.0 * Constants.GodotToTb

func update_properties():
    if not Engine.is_editor_hint():
        return

    for child in get_children():
        remove_child(child)
        child.queue_free()
    
    # directional light settings
    var angles: Vector3 = properties.get("angles", Vector3.ZERO)
    # TB pitch is clockwise instead of counter clockwise, so we negate them
    angles.x = -angles.x
    angles.y += 180 # TODO: is this correct? the worldspawn mesh is kinda broken
    var sky_mode: DirectionalLight3D.SkyMode = properties.get("sky_mode", 0)
    
    # General Light settings
    var color: Color = properties.get("color", Color.WHITE)
    var energy: float = properties.get("energy", 1.0)
    var is_short_lived: bool = properties.get("short_lived", false)
    var volumetric_fog_energy := 0.0 if is_short_lived else energy
    var bake_mode: int = properties.get("bake_mode", Light3D.BAKE_DYNAMIC)
    
    # Shadow settings
    var shadows_enabled: bool = properties.get("shadows", false)
    
    # TODO: directional shadow settings
    
    var light_node := DirectionalLight3D.new()
    light_node.rotation_degrees = angles
    light_node.sky_mode = sky_mode
    
    light_node.light_color = color
    light_node.light_energy = energy
    light_node.light_indirect_energy = energy
    light_node.light_volumetric_fog_energy = volumetric_fog_energy
    light_node.light_bake_mode = bake_mode
    
    light_node.shadow_enabled = shadows_enabled

    add_child(light_node)
    if is_inside_tree():
        var tree = get_tree()
        if tree:
            var edited_scene_root = tree.get_edited_scene_root()
            if edited_scene_root:
                light_node.set_owner(edited_scene_root)
