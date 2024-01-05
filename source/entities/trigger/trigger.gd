@tool
extends Area3D

# defaults to just the player layer
const DEFAULT_MASK := 1 << 1

@export var properties: Dictionary:
    get:
        return properties  # TODO Converter40 Non existent get function
    set(new_properties):
        if properties != new_properties:
            properties = new_properties
            update_properties()

signal trigger()

func update_properties():
    if not Engine.is_editor_hint():
        return

    collision_mask = properties.get("collision_mask", DEFAULT_MASK)
    collision_layer = 0

func _ready():
    body_entered.connect(_body_entered)

func _body_entered():
    # TODO: store the body that triggered this I/O
    trigger.emit()
