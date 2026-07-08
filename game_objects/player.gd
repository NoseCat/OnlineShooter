extends CharacterBody3D


const SPEED = 5.0
const JUMP_VELOCITY = 4.5

var test

func _ready() -> void:
	#setting up Syncronizer so it knows what properties to copy
	$MultiplayerSynchronizer.root_path = ".."
	var replication_config = SceneReplicationConfig.new()
	replication_config.add_property(":global_position")
	replication_config.add_property(":rotation")
	$MultiplayerSynchronizer.replication_config = replication_config
	
	if not is_multiplayer_authority():
		return
	$Camera3D.current = true
	test = $test

func _physics_process(delta: float) -> void:
	if not is_multiplayer_authority():
		return
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta
	
	# Handle jump.
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Get the input direction and handle the movement/deceleration.
	# As good practice, you should replace UI actions with custom gameplay actions.
	var input_dir := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)
		velocity.z = move_toward(velocity.z, 0, SPEED)
		
	if(Input.is_action_just_pressed("LMB")):
		rpc("click")
	
	move_and_slide()

@rpc("any_peer", "call_local")
func click():
#	if not is_multiplayer_authority():
#		return
	#test.mesh.radius = 0.1
	print(get_multiplayer_authority())
	print(is_multiplayer_authority())
