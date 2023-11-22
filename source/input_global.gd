extends Node

func _input(event: InputEvent):
    if Input.mouse_mode != Input.MOUSE_MODE_CAPTURED:
        if event is InputEventMouseButton:
            if event.button_index == MOUSE_BUTTON_LEFT:
                Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
    elif event is InputEventKey:
        if event.physical_keycode == KEY_ESCAPE:
            Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
