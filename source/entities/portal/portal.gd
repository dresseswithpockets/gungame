class_name Portal extends Node3D

@export var player_group: StringName = &"player"
@export_node_path("Portal") var linked_portal_path: NodePath

@onready var sub_viewport: SubViewport = $PortalSubViewport
@onready var viewport_cam: Camera3D = sub_viewport.get_camera_3d()

@onready var front: Node3D = $FrontSurface
@onready var back: Node3D = $BackSurface
@onready var surface: Node3D = $Surface

@onready var area: Area3D = $Area3D

var linked_portal: Portal
var is_linked: bool = false
var tracking: Array[PhysicsBody3D] = []
var tracking_last_side: Array[int] = []
var player_tracked: bool = false

# Called when the node enters the scene tree for the first time.
func _ready():
    var node = get_node_or_null(linked_portal_path)
    if node == null:
        set_physics_process(false)
        return
        
    assert(node != self)

    var player_camera: Camera3D = get_tree().get_first_node_in_group(player_group).camera
    viewport_cam.fov = player_camera.fov
    # TODO: what about downsampling? this probably works even in that case tho
    sub_viewport.size = get_tree().get_root().get_viewport().size
    
    linked_portal = node
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
        var offset_from_portal = body.global_position - global_position
        
        var new_side: int = sign(offset_from_portal.dot(-global_transform.basis.z))
        
        # the tracked body has crossed sides in the past frame, so we can
        # teleport them
        if new_side != previous_side:
            # does this teleport?
            var t = global_transform.affine_inverse() * linked_portal.global_transform * body.global_transform
            body.global_transform = t
            
            if body is Player:
                player_tracked = false
            tracking.remove_at(index)
            tracking_last_side.remove_at(index)
            index -= 1
            
            # need to inform the other portal of what side the body just came from
            linked_portal.tracking.append(body)
            linked_portal.tracking_last_side.append(new_side)
        else:
            tracking_last_side[index] = new_side
        index += 1
    
    #protect_screen_clipping()
    fix_near_plane_clip()

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

func protect_screen_clipping():
    # only do anti-screen clip if the player is being tracked
    if player_tracked:
        var player_cam: Camera3D = get_tree().get_first_node_in_group(player_group).camera
        var player_cam_proj := player_cam.get_camera_projection()
        var half_width: float
        var half_height: float
        if player_cam.keep_aspect == Camera3D.KEEP_HEIGHT:
            half_width = player_cam.near * tan(deg_to_rad(player_cam.fov * 0.5))
            half_height = half_width * (1 / player_cam_proj.get_aspect())
        else:
            half_height = player_cam.near * tan(deg_to_rad(player_cam.fov * 0.5))
            half_width = half_height * player_cam_proj.get_aspect()
        var near_clip_plane_distance := Vector3(half_width, half_height, player_cam.near).length()
        
        var offset := global_position - player_cam.global_position
        var facing_same_direction_sign: int = sign(global_transform.basis.z.dot(offset))
        surface.scale = Vector3(surface.scale.x, surface.scale.y, near_clip_plane_distance)
        surface.position = Vector3.FORWARD * near_clip_plane_distance * \
                        0.5 * facing_same_direction_sign
    else:
        surface.scale = Vector3(1, 1, 0.2)
        surface.position = Vector3.ZERO
#    front.position = Vector3.FORWARD * near_clip_plane_distance
#    back.position = Vector3.FORWARD * near_clip_plane_distance
#    if is_facing_same_direction:
#        front.position *= 0.5
#        back.position *= -0.5
#    else:
#        front.position *= -0.5
#        back.position *= 0.5

func on_body_entered(body: PhysicsBody3D):
    if body in tracking: return
    if body is Player:
        player_tracked = true
    tracking.append(body)
    var offset_from_portal := body.global_position - global_position
    var side: int = sign(offset_from_portal.dot(-global_transform.basis.z))
    tracking_last_side.append(side)

func on_body_exited(body: PhysicsBody3D):
    var index := tracking.find(body)
    if index == -1: return
    if body is Player:
        player_tracked = false
    tracking.remove_at(index)
    tracking_last_side.remove_at(index)
