extends RigidBody3D

var speed := 1.0

func _physics_process(delta: float):
    if Input.is_key_pressed(KEY_UP):
        translate(Vector3.FORWARD * speed * delta)
    elif Input.is_key_pressed(KEY_DOWN):
        translate(-Vector3.FORWARD * speed * delta)
