extends Node

@export var map_folder: String = "res://maps/"  

func load_map(map_name: String) -> void:
	var map_path = map_folder + map_name + ".tscn"
	var map_scene = load(map_path)
	if map_scene == null:
		push_error("Map not found: ", map_path)
		return
	var map_instance = map_scene.instantiate()
	add_child(map_instance)
	
