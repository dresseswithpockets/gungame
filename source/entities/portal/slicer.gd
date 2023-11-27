class_name Slicer extends Node3D

@onready var slice_shader: Shader = preload("res://entities/portal/slice.gdshader")

@export var graphics_node: Node3D
var clone_node: Node3D

var main_materials: Array[ShaderMaterial]
var clone_materials: Array[ShaderMaterial]

func enter_portal_threshold():
    if clone_node == null:
        clone_node = graphics_node.duplicate(0)
        graphics_node.add_child(clone_node)
        main_materials = []
        _get_all_slice_materials(graphics_node, main_materials)
        clone_materials = []
        _get_all_slice_materials(clone_node, clone_materials)
    else:
        graphics_node.add_child(clone_node)
        clone_node.global_transform = Transform3D.IDENTITY

func exit_portal_threshold():
    graphics_node.remove_child(clone_node)
    for mat in main_materials:
        mat.set_shader_parameter("slice_normal", Vector3.ZERO)

func _get_all_slice_materials(in_node: Node, array: Array[ShaderMaterial]):
    if in_node is GeometryInstance3D:
        var geo_instance: GeometryInstance3D = in_node
        if in_node.material_override is ShaderMaterial:
            var material: ShaderMaterial = in_node.material_override
            if material.shader == slice_shader:
                array.push_back(in_node.material_override)
    for child in in_node.get_children():
        _get_all_slice_materials(child, array)
