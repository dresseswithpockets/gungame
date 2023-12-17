@tool
extends QodotEntity

const default_radius: float = 5.0 * Constants.GodotToTb

func update_properties():
    if not Engine.is_editor_hint():
        return

    for child in get_children():
        remove_child(child)
        child.queue_free()
    
    # Spot settings
    var spot_range: float = properties.get("spot_range", default_radius)
    spot_range *= Constants.TbToGodot
    var spot_angle: float = properties.get("spot_angle", 45.0)
    var spot_attenuation: float = properties.get("spot_attenuation", 1.0)
    var spot_angle_attenuation: float = properties.get("spot_angle_attenuation", 1.0)
    
    # General Light settings
    var color: Color = properties.get("color", Color.WHITE)
    var energy: float = properties.get("energy", 1.0)
    var is_short_lived: bool = properties.get("short_lived", false)
    var volumetric_fog_energy := 0.0 if is_short_lived else energy
    var bake_mode: int = properties.get("bake_mode", Light3D.BAKE_DYNAMIC)
    
    # Shadow settings
    var shadows_enabled: bool = properties.get("shadows", false)
    
    var light_node := SpotLight3D.new()
    light_node.spot_angle = spot_angle
    light_node.spot_angle_attenuation = spot_angle_attenuation
    light_node.spot_attenuation = spot_attenuation
    light_node.spot_range = spot_range
    
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
