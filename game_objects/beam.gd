extends Node3D

@onready var cyl = $Cylinder
@onready var timer = $Timer

func init(start: Vector3, end: Vector3):
	look_at(end)
	cyl.mesh.height = (start - end).length()
	top_level = true
	global_position = start + (end - start)/2
	timer.start()

func _on_timer_timeout():
	queue_free()
