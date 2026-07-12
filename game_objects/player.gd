extends CharacterBody3D


const SPEED = 5.0
const JUMP_VELOCITY = 4.5

@export var max_health:float = 50.0
var health = max_health

var test

@export var lobby_id: int
@onready var main = $"/root/main"


@export var particles_scene: PackedScene 



func _ready() -> void:
	#setting up Syncronizer so it knows what properties to copy
	var sync = $MultiplayerSynchronizer
	sync.root_path = ".."
	var replication_config = SceneReplicationConfig.new()
	replication_config.add_property(":global_position")
	replication_config.add_property(":rotation")
	replication_config.add_property(":health")
	sync.replication_config = replication_config
	
	var lobby_index = main.GetLobbyIndexById(lobby_id)
	var LobbyPlayers = main.LobbyData[lobby_index]["Players"]
	for playerid in LobbyPlayers:
		sync.set_visibility_for(playerid, true)
	sync.set_visibility_for(1, true)

func _physics_process(delta: float) -> void:

	if not (name.to_int() == multiplayer.get_unique_id()):
		return
		
	if(Input.is_action_just_pressed("LMB")):
		#rpc("take_damage", 1)
		#take_damage(1)
		rpc("click")
	

# Called when the local player clicks
@rpc("any_peer", "call_local")
func click():
	if not (name.to_int() == multiplayer.get_unique_id()):
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
