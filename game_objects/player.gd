extends CharacterBody3D


const SPEED = 5.0
const JUMP_VELOCITY = 4.5

@export var max_health:float = 50.0
var health = max_health

var test

@export var lobby_id: int
@onready var main = $"/root/main"


@export var particles_scene: PackedScene 

@rpc("authority", "call_local")
func set_authority(id: int):
	set_multiplayer_authority(id)

@onready var sync = $MultiplayerSynchronizer

@rpc("any_peer", "call_local")
func update_visibility():
	for lobby in main.LobbyData:
		var LobbyIPlayers = lobby["Players"]
		for playerid in LobbyIPlayers:
			sync.set_visibility_for(playerid, false)
	
	var lobby_index = main.GetLobbyIndexById(lobby_id)
	var LobbyPlayers = main.LobbyData[lobby_index]["Players"]
	for playerid in LobbyPlayers:
		sync.set_visibility_for(playerid, true)
	sync.set_visibility_for(1, true)

@rpc("any_peer", "call_remote")
func set_up():
	$Camera3D.current = true

func _ready() -> void:
	#setting up Syncronizer so it knows what properties to copy
	sync.root_path = ".."
	var replication_config = SceneReplicationConfig.new()
	replication_config.add_property(":global_position")
	replication_config.add_property(":rotation")
	replication_config.add_property(":health")
	sync.replication_config = replication_config
	update_visibility()
	
	set_process_input(is_multiplayer_authority()) 
	
	#if is_multiplayer_authority():

func _physics_process(delta: float) -> void:
	#print(get_multiplayer_authority())
	if not is_multiplayer_authority():
		return
	
	if Input.is_action_just_pressed("Esc"):
		$EscMenu.visible = !$EscMenu.visible
		if ($EscMenu.visible):
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			
	
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

	spawn_particles.rpc(from, to)

	var collider = $Camera3D/RayCast3D.get_collider()

	if collider is CharacterBody3D and collider.has_method("take_damage"):
		collider.take_damage.rpc(1)

# RPC to spawn beam on all peers
@rpc("any_peer", "call_local")
func spawn_particles(start: Vector3, end: Vector3):
	var particles = particles_scene.instantiate()
	get_tree().root.add_child(particles) 
	particles.init(start, end)

@rpc("any_peer", "call_local")
func take_damage(value):
	health -= value

func _on_button_pressed() -> void:
	main.DisconnectClient(lobby_id)
