class_name Portal extends Node3D

@export var player_group: StringName = &"player"
# NOTE: have to use NodePath instead of a direct Node reference because of a 
# ref-loop bug in Godot when setting a breakpoint
@export_node_path var linked_portal_path: NodePath

@onready var sub_viewport: SubViewport = $PortalViewportDepth1
@onready var viewport_cam: Camera3D = $PortalViewportDepth1/Camera

@onready var surface: Node3D = $SurfaceDepth1

@onready var area: Area3D = $Area3D

var linked_portal: Portal
var is_linked: bool = false
var tracking: Array[PhysicsBody3D] = []
var tracking_last_side: Array[int] = []
var tracking_slicer: Array = []

@onready var portal_shader: Shader = preload("res://entities/portal/fixed_shader.gdshader")
@onready var slice_shader: Shader = preload("res://entities/portal/slice.gdshader")

# Called when the node enters the scene tree for the first time.
func _ready():
    var node = get_node_or_null(linked_portal_path)
    if node == null:
        set_physics_process(false)
        return
        
    assert(node != self)
    linked_portal = node

    var player_camera: Camera3D = get_tree().get_first_node_in_group(player_group).camera
    viewport_cam.fov = player_camera.fov
    sub_viewport.size = get_tree().get_root().get_viewport().size
    
    is_linked = true
    area.body_entered.connect(on_body_entered)
    area.body_exited.connect(on_body_exited)
    set_physics_process(true)

func _process(_delta):
    if !is_linked or !get_tree().has_group(player_group): return
    var player_camera: Camera3D = get_tree().get_first_node_in_group(player_group).camera
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
    var player_cam: Camera3D = get_tree().get_first_node_in_group(player_group).camera
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
