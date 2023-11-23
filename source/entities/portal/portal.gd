class_name Portal extends Node3D

@export var player_group: StringName = &"player"
@export_node_path("Portal") var linked_portal_path: NodePath

@onready var sub_viewport: SubViewport = $PortalSubViewport
@onready var viewport_cam: Camera3D = sub_viewport.get_camera_3d()
@onready var surface: Node3D = $PortalSurface

var linked_portal: Portal
var is_linked: bool = false

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
    set_physics_process(true)

func _process(_delta):
    if !is_linked or !get_tree().has_group(player_group): return
    var player_camera: Camera3D = get_tree().get_first_node_in_group(player_group).camera
    var m = global_transform.inverse() * linked_portal.global_transform * player_camera.global_transform
    viewport_cam.global_transform = m
