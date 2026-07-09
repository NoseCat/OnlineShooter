extends CharacterBody3D


const SPEED = 5.0
const JUMP_VELOCITY = 4.5

@export var max_health:float = 50.0
var health = max_health

var test

# ... your existing variables ...

@export var beam_scene: PackedScene  # assign Beam.tscn in the inspector

func _ready() -> void:
	#setting up Syncronizer so it knows what properties to copy
	$MultiplayerSynchronizer.root_path = ".."
	var replication_config = SceneReplicationConfig.new()
	replication_config.add_property(":global_position")
	replication_config.add_property(":rotation")
	replication_config.add_property(":health")
	$MultiplayerSynchronizer.replication_config = replication_config
	
	set_process_input(is_multiplayer_authority()) 
	
	if not is_multiplayer_authority():
		return
	$Camera3D.current = true
	test = $test
	test.mesh = test.mesh.duplicate()

func _physics_process(delta: float) -> void:
	if not is_multiplayer_authority() or multiplayer.is_server():
		return

	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta
	
	# Handle jump.
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	var input_dir := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)
		velocity.z = move_toward(velocity.z, 0, SPEED)
		
	if(Input.is_action_just_pressed("LMB")):
		#rpc("take_damage", 1)
		#take_damage(1)
		rpc("click")
	
	move_and_slide()

# Called when the local player clicks
@rpc("any_peer", "call_local")
func click():
	if not is_multiplayer_authority():
		return  # only the owning client should trigger this

	if(not $Camera3D/RayCast3D.is_colliding()):
		return

	var from = $Camera3D.global_position
	var to = $Camera3D/RayCast3D.get_collision_point()

#do particles instead
	rpc("spawn_beam", from, to)

	var collider = $Camera3D/RayCast3D.get_collider()

	if collider is CharacterBody3D and collider.has_method("take_damage"):
		collider.take_damage.rpc(1)

# RPC to spawn beam on all peers
@rpc("any_peer", "call_local")
func spawn_beam(start: Vector3, end: Vector3):
	var beam = beam_scene.instantiate()
	get_tree().root.add_child(beam)  # add to root so it's not affected by parent transform
	beam.init(start, end)

@rpc("any_peer", "call_local")
func take_damage(value):
	health -= value
