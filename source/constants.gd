extends Node

const TbUnitsPerGodot: float = 128.0
const GodotUnitsPerTb: float = 1.0 / TbUnitsPerGodot

const GodotToTb: float = TbUnitsPerGodot
const TbToGodot: float = GodotUnitsPerTb

var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")
