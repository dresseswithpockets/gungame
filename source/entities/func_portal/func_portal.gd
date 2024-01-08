@tool
class_name FuncPortal extends Area3D

@export var player_cam_group: StringName = &"player_cam"
@export var linked_portal: FuncPortal

@export var sub_viewport: SubViewport
@export var viewport_cam: Camera3D

@export var surface: GeometryInstance3D

var is_linked: bool:
    get:
        return linked_portal != null
var tracking: Array[PhysicsBody3D] = []
var tracking_last_side: Array[int] = []
var tracking_slicer: Array = []

@onready var portal_shader: Shader = preload("res://entities/portal/fixed_shader.gdshader")
@onready var slice_shader: Shader = preload("res://entities/portal/slice.gdshader")
@onready var members_prefab: PackedScene = preload("res://entities/func_portal/func_portal_members.tscn")

@export var properties: Dictionary:
    get:
        return properties
    set(new_properties):
        if properties != new_properties:
            properties = new_properties
            update_properties()

func update_properties():
    if not Engine.is_editor_hint():
        return

    if sub_viewport:
        remove_child(sub_viewport)
        sub_viewport.queue_free()
    
    var self_name = properties.get("targetname", name)
    var target_portal = properties.get("target")
    if target_portal == null or target_portal == "":
        push_warning("'%s' has no target portal." % self_name)
        return

    target_portal = get_parent().get_nodes_by_targetname(target_portal)
    if len(target_portal) > 1:
        push_warning("'%s' target points to multiple entities. Will only link to the first one found." % self_name)
    elif len(target_portal) == 0:
        push_warning("'%s' has no target portal." % self_name)
        return

    target_portal = target_portal[0]
    if target_portal == self:
        push_error("'%s' targets itself, but should target a different func_portal" % self_name)
        return
    
    linked_portal = target_portal
    
    # the first mesh instance is used as the surface
    for child in get_children():
        if child is MeshInstance3D:
            surface = child
            break
    
    var members = members_prefab.instantiate()
    add_child(members)
    if is_inside_tree():
        var tree = get_tree()
        if tree:
            var edited_scene_root = tree.get_edited_scene_root()
            if edited_scene_root:
                members.set_owner(edited_scene_root)
    
    sub_viewport = members.sub_viewport
    viewport_cam = members.viewport_cam
    
    var shader_material = ShaderMaterial.new()
    shader_material.shader = portal_shader
    shader_material.set_shader_parameter("texture_albedo", sub_viewport.get_texture())
    surface.material_override = shader_material

    #sub_viewport = SubViewport.new()
    #sub_viewport.name = "SubViewport1"
    #sub_viewport.size = get_viewport().size
    #sub_viewport.handle_input_locally = false
    #sub_viewport.gui_disable_input = true
    #viewport_cam = Camera3D.new()
    #viewport_cam.name = "ViewportCam"
    #viewport_cam.current = true
    #sub_viewport.add_child(viewport_cam)
    #add_child(sub_viewport)
    #if is_inside_tree():
        #var tree = get_tree()
        #if tree:
            #var edited_scene_root = tree.get_edited_scene_root()
            #if edited_scene_root:
                #sub_viewport.set_owner(edited_scene_root)
    
    #surface.material_override = ShaderMaterial.new()
    #surface.material_override.setup_local_to_scene()


# Called when the node enters the scene tree for the first time.
func _ready():
    if Engine.is_editor_hint():
        return

    get_viewport().size_changed.connect(_on_viewport_size_changed)

    var player_camera: Camera3D = get_tree().get_first_node_in_group(player_cam_group)
    viewport_cam.fov = player_camera.fov
    sub_viewport.size = get_tree().get_root().get_viewport().size
    
    body_entered.connect(on_body_entered)
    body_exited.connect(on_body_exited)
    set_physics_process(true)

func _on_viewport_size_changed():
    sub_viewport.size = get_viewport().size

func _process(_delta):
    if Engine.is_editor_hint():
        return
    
    if !is_linked or !get_tree().has_group(player_cam_group): return
    var player_camera: Camera3D = get_tree().get_first_node_in_group(player_cam_group)
    var m = global_transform.inverse() * linked_portal.global_transform * player_camera.global_transform
    viewport_cam.global_transform = m
    
    var index = 0
    while index < len(tracking):
        var body := tracking[index]
        var previous_side := tracking_last_side[index]
        var slicer = tracking_slicer[index]
        var new_side := side_of_portal(body.global_position)
        
        var t = global_transform.affine_inverse() * linked_portal.global_transform * body.global_transform
        # the tracked body has crossed sides in the past frame, so we can
        # teleport them
        if new_side != previous_side:
            # does this teleport?
            body.global_transform = t
            
            tracking.remove_at(index)
            tracking_last_side.remove_at(index)
            index -= 1
            
            # need to inform the other portal of what side the body just came from
            #linked_portal.tracking.append(body)
            #linked_portal.tracking_last_side.append(new_side)
        else:
            if slicer != null:
                slicer.clone_node.global_transform = t
                update_slice_params(slicer)
            tracking_last_side[index] = new_side
        index += 1
    
#    for list in tracked_slice_materials:
#        for item in list:
#            var slice_mat: ShaderMaterial = item
#            slice_mat.set_shader_parameter("slice_center", global_position)
#            slice_mat.set_shader_parameter("slice_normal", -global_transform.basis.z)

    fix_near_plane_clip()

func update_slice_params(slicer: Slicer):
    var side := side_of_portal(slicer.global_position)
    var slice_normal := -global_transform.basis.z * -side
    var clone_slice_normal := -linked_portal.global_transform.basis.z * side
    
    var slice_pos := global_position
    var clone_slice_pos := linked_portal.global_position
    
    for mat in slicer.main_materials:
        mat.set_shader_parameter("slice_center", slice_pos)
        mat.set_shader_parameter("slice_normal", slice_normal)
        # TODO: slice_offset_dst
    
    for mat in slicer.clone_materials:
        mat.set_shader_parameter("slice_center", clone_slice_pos)
        mat.set_shader_parameter("slice_normal", clone_slice_normal)
        # TODO: slice_offset_dst

func side_of_portal(pos: Vector3) -> int:
    var offset_from_portal = pos - global_position
    return sign(offset_from_portal.dot(-global_transform.basis.z))

func fix_near_plane_clip():
    var player_cam: Camera3D = get_tree().get_first_node_in_group(player_cam_group)
    var player_cam_proj := player_cam.get_camera_projection()
    var aspect := player_cam_proj.get_aspect()
    var half_width: float
    var half_height: float
    if player_cam.keep_aspect == Camera3D.KEEP_HEIGHT:
        half_width = player_cam.near * tan(deg_to_rad(player_cam.fov * 0.5))
        half_height = half_width * (1 / aspect)
    else:
        half_height = player_cam.near * tan(deg_to_rad(player_cam.fov * 0.5))
        half_width = half_height * aspect
    
    var distance_to_near_clip_plane_corner := Vector3(half_width, half_height, player_cam.near).length()
    
    var forward := -global_transform.basis.z
    var player_cam_offset := global_position - player_cam.global_position
    var cam_facing_sam_dir_as_portal := forward.dot(player_cam_offset) > 0
    surface.scale.z = distance_to_near_clip_plane_corner
    surface.position = Vector3.FORWARD * distance_to_near_clip_plane_corner * (0.5 if cam_facing_sam_dir_as_portal else -0.5)

func on_body_entered(body: PhysicsBody3D):
    if body in tracking: return
    
    var slicer = _get_slicer_or_null(body)
    if slicer != null:
        slicer.enter_portal_threshold()
        
    tracking.append(body)
    tracking_last_side.append(side_of_portal(body.global_position))
    tracking_slicer.append(slicer)

func on_body_exited(body: PhysicsBody3D):
    var index := tracking.find(body)
    if index == -1: return
    
    var slicer = tracking_slicer[index]
    if slicer != null:
        slicer.exit_portal_threshold()

    tracking.remove_at(index)
    tracking_last_side.remove_at(index)
    tracking_slicer.remove_at(index)

func _get_slicer_or_null(in_node: Node):
    if in_node is Slicer:
        return in_node
    for child in in_node.get_children():
        return _get_slicer_or_null(child)
