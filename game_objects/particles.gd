extends Node3D

@onready var timer = $Timer
@onready var particles = $CPUParticles3D

func init(start: Vector3, end: Vector3):
	look_at(start)
	particles.emitting = true
	global_position = end
	timer.start()

func _on_timer_timeout():
	queue_free()
