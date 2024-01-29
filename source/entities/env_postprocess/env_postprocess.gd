@tool
class_name EnvPostprocessOld extends QodotEntity

func update_properties():
    if not Engine.is_editor_hint():
        return
    
    var parent := find_parent("QodotMap")
    if parent != null:
        for child in parent.get_children():
            if child != self and child is EnvPostprocessOld:
                push_warning("Multiple env_postprocess entities detected. Multiple env_postprocess may lead to unexpected behaviour.")
    
    # the docs say there can only be one WorldEnvironment in the scene tree...
    # but, I've instanced multiple in the tree at once with no problem
    for child in get_children():
        remove_child(child)
        child.queue_free()
    
    var env := Environment.new()
    env.adjustment_enabled = properties.get("adjustment_enabled", false)
    # NOTE: adjustment_color_correction is ignored
    env.adjustment_brightness = properties.get("adjustment_brightness", 1.0)
    env.adjustment_contrast = properties.get("adjustment_contrast", 1.0)
    env.adjustment_saturation = properties.get("adjustment_saturation", 1.0)
    
    env.ambient_light_source = properties.get("ambient_light_source", 0)
    env.ambient_light_color = properties.get("ambient_light_color", Color.BLACK)
    env.ambient_light_energy = properties.get("ambient_light_energy", 1.0)
    env.ambient_light_sky_contribution = properties.get("ambient_light_sky_contribution", 1.0)
    
    # TODO: support other background modes, for now only sky is supported
    env.background_mode = Environment.BG_SKY
    env.background_energy_multiplier = properties.get("background_energy_multiplier", 1.0)
    
    env.fog_enabled = properties.get("fog_enabled", false)
    env.fog_aerial_perspective = properties.get("fog_aerial_perspective", 0.0)
    env.fog_density = properties.get("fog_density", 0.01)
    env.fog_height = properties.get("fog_height", 0.0)
    env.fog_height_density = properties.get("fog_height_density", 0.0)
    env.fog_light_color = properties.get("fog_light_color", env.fog_light_color)
    env.fog_light_energy = properties.get("fog_light_energy", 1.0)
    env.fog_sky_affect = properties.get("fog_sky_affect", 1.0)
    env.fog_sun_scatter = properties.get("fog_sun_scatter", 0.0)
    
    # TODO: support glow
    
    env.reflected_light_source = properties.get("reflected_light_source", 0)
    
    # TODO: add sky properties
    env.sky = Sky.new()
    env.sky.sky_material = ProceduralSkyMaterial.new()
    
    env.ssao_enabled = properties.get("ssao_enabled", false)
    env.ssao_ao_channel_affect = properties.get("ssao_ao_channel_affect", 0.0)
    env.ssao_detail = properties.get("ssao_detail", 0.5)
    env.ssao_horizon = properties.get("ssao_horizon", 0.06)
    env.ssao_intensity = properties.get("ssao_intensity", 2.0)
    env.ssao_light_affect = properties.get("ssao_light_affect", 0.0)
    env.ssao_power = properties.get("ssao_power", 1.5)
    env.ssao_radius = properties.get("ssao_radius", 1.0)
    env.ssao_sharpness = properties.get("ssao_sharpness", 0.98)
    
    env.ssil_enabled = properties.get("ssil_enabled", false)
    env.ssil_intensity = properties.get("ssil_intensity", 1.0)
    env.ssil_normal_rejection = properties.get("ssil_normal_rejection", 1.0)
    env.ssil_radius = properties.get("ssil_radius", 5.0)
    env.ssil_sharpness = properties.get("ssil_sharpness", 0.98)
    
    env.ssr_enabled = properties.get("ssr_enabled", false)
    env.ssr_depth_tolerance = properties.get("ssr_depth_tolerance", 0.2)
    env.ssr_fade_in = properties.get("ssr_fade_in", 0.15)
    env.ssr_fade_out = properties.get("ssr_fade_out", 2.0)
    env.ssr_max_steps = properties.get("ssr_max_steps", 64)
    
    env.tonemap_exposure = properties.get("tonemap_exposure", 1.0)
    env.tonemap_mode = properties.get("tonemap_mode", 0)
    env.tonemap_white = properties.get("tonemap_white", 1.0)
    
    env.volumetric_fog_enabled = properties.get("volumetric_fog_enabled", false)
    env.volumetric_fog_albedo = properties.get("volumetric_fog_albedo", Color.WHITE)
    env.volumetric_fog_ambient_inject = properties.get("volumetric_fog_ambient_inject", 0.0)
    env.volumetric_fog_anisotropy = properties.get("volumetric_fog_anisotropy", 0.2)
    env.volumetric_fog_density = properties.get("volumetric_fog_density", 0.05)
    env.volumetric_fog_detail_spread = properties.get("volumetric_fog_detail_spread", 2.0)
    env.volumetric_fog_emission = properties.get("volumetric_fog_emission", Color.BLACK)
    env.volumetric_fog_emission_energy = properties.get("volumetric_fog_emission_energy", 1.0)
    env.volumetric_fog_gi_inject = properties.get("volumetric_fog_gi_inject", 1.0)
    env.volumetric_fog_length = properties.get("volumetric_fog_length", 64.0)
    env.volumetric_fog_sky_affect = properties.get("volumetric_fog_sky_affect", 1.0)
    
    env.volumetric_fog_temporal_reprojection_enabled = properties.get("volumetric_fog_temporal_reprojection_enabled", false)
    env.volumetric_fog_temporal_reprojection_amount = properties.get("volumetric_fog_temporal_reprojection_amount", 0.9)
    
    var world_env := WorldEnvironment.new()
    world_env.environment = env

    add_child(world_env)
    if is_inside_tree():
        var tree = get_tree()
        if tree:
            var edited_scene_root = tree.get_edited_scene_root()
            if edited_scene_root:
                world_env.set_owner(edited_scene_root)
