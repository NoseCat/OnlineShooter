extends Camera3D

var sensitivity := 0.003

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _input(event):
	if event is InputEventMouseMotion and current:
		var parent = get_parent()
		if parent:
			parent.rotate_y(-event.relative.x * sensitivity)
		rotate_x(-event.relative.y * sensitivity)
		rotation.x = clamp(rotation.x, deg_to_rad(-90.0), deg_to_rad(90.0))
