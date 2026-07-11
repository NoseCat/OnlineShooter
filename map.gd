#extends Node
#
#func _ready() -> void:
	## Path to the maps folder
	#const MAPS_PATH = "res://maps/"
	#
	## Use DirAccess to list all files in the folder
	#var dir = DirAccess.open(MAPS_PATH)
	#if dir == null:
		#push_error("Failed to open maps folder: ", MAPS_PATH)
		#return
	#
	## Filter only .tscn (or .scn) scene files
	#var scene_files: Array[String] = []
	#dir.list_dir_begin()
	#var file_name = dir.get_next()
	#while file_name != "":
		#if not dir.current_is_dir() and (file_name.ends_with(".tscn") or file_name.ends_with(".scn")):
			#scene_files.append(file_name)
		#file_name = dir.get_next()
	#dir.list_dir_end()
	#
	#if scene_files.is_empty():
		#push_error("No scene files found in: ", MAPS_PATH)
		#return
	#
	## Pick a random scene file
	#var random_index = randi() % scene_files.size()
	#var selected_file = scene_files[random_index]
	#var full_path = MAPS_PATH + selected_file
	#
	## Load and instantiate the scene
	#var scene_resource = load(full_path)
	#if scene_resource == null:
		#push_error("Failed to load scene: ", full_path)
		#return
	#
	#var map_instance = scene_resource.instantiate()
	#if map_instance == null:
		#push_error("Failed to instantiate scene: ", full_path)
		#return
	#
	## Add as child
	#add_child(map_instance)
	## Optionally reset its transform to identity (if desired)
	## map_instance.transform = Transform3D.IDENTITY
#
#
## _process is not needed for this functionality; you can remove it
## func _process(delta: float) -> void:
##     pass
