@tool
extends QodotEntity

func update_properties():
    pass

func use(activator: Node3D):
    var message = properties.get("message", "")
    var print_activator = properties.get("print_activator", false)
    if print_activator:
        message = "debug_print activated by '%s': %s" % [activator.name, message]
    else:
        message = "debug_print: %s" % message

    ConsoleGlobal.PushLine(message, 0)
    print(message)
