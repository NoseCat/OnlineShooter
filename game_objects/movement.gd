extends Node3D
#
#
#const SPEED = 5.0
#const JUMP_VELOCITY = 4.5
#
#var test
#
#@onready var player = $".."
#
#@export var particles_scene: PackedScene 
#
#
#var is_LMB_just_pressed = false
#var input_dir = Vector2(0, 0)
#
#@rpc("any_peer", "call_remote")
#func set_actions(is_LMB: bool, input: Vector2):
	#input_dir = input
	#is_LMB_just_pressed = is_LMB
#
#@rpc("any_peer", "call_remote")
#func srvrPLS(method: String, object: Node): #hack
	#object.call(method)
#
#func _ready() -> void:
	#
	##is_multiplayer_authority()
	#if not (player.name.to_int() == multiplayer.get_unique_id()):
		#return
	#$"../Camera3D".current = true
#
#@rpc("any_peer", "call_remote")
#func _physics_process(delta: float) -> void:
	##if not (name.to_int() == multiplayer.get_unique_id()):
	##	return
	#if (not multiplayer.is_server()):
		#rpc_id(1, "srvrPLS", "_physics_process", self);
		#rpc_id(1, "set_actions", Input.is_action_just_pressed("ui_accept"), Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down"));
	#
	## Add the gravity.
	#if not player.is_on_floor():
		#player.velocity += player.get_gravity() * delta
	#
	## Handle jump.
	#if is_LMB_just_pressed and player.is_on_floor():
		#player.velocity.y = JUMP_VELOCITY
#
	#var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	#if direction:
		#player.velocity.x = direction.x * SPEED
		#player.velocity.z = direction.z * SPEED
	#else:
		#player.velocity.x = move_toward(player.velocity.x, 0, SPEED)
		#player.velocity.z = move_toward(player.velocity.z, 0, SPEED)
	#
	#player.move_and_slide()
