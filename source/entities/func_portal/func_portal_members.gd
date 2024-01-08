@tool
extends Node3D

@export var material: ShaderMaterial
@export var sub_viewport: SubViewport:
    get:
        return $PortalViewportDepth1
@export var viewport_cam: Camera3D:
    get:
        return $PortalViewportDepth1/Camera
