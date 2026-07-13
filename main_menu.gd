extends Control

@onready var main_node = $".."

# UI elements
@onready var lobby_list = $VBoxContainer/ScrollContainer/LobbyList
@onready var refresh_btn = $VBoxContainer/RefreshButton
@onready var join_btn = $VBoxContainer/JoinButton
@onready var create_btn = $VBoxContainer/CreateButtom

var selected_lobby_id: int = -1

func _ready():
	# Connect button signals
	refresh_btn.pressed.connect(_on_refresh_pressed)
	join_btn.pressed.connect(_on_join_pressed)
	create_btn.pressed.connect(_on_create_pressed)
	# Disable Join button until a lobby is selected
	join_btn.disabled = true
	# Auto‑refresh when the menu appears
	_refresh_lobby_list()

# Clears and rebuilds the lobby list from the server's data
func _refresh_lobby_list():
	# Clear existing buttons
	for child in lobby_list.get_children():
		child.queue_free()
	
	# Get the current LobbyData from the C# Main node
	# LobbyData is a Godot.Collections.Array of Dictionaries
	var lobby_data = main_node.LobbyData
	if lobby_data == null or lobby_data.size() == 0:
		_add_no_lobbies_label()
		return
	
	# Create a button for each lobby
	for i in range(lobby_data.size()):
		var lobby_dict = lobby_data[i]
		var lobby_id =  lobby_dict.get("Id")
		var lobby_name = lobby_dict.get("Name", "Unnamed")
		var lobby_map = lobby_dict.get("Map")
		var player_count = lobby_dict.get("Players", []).size()
		var max_players = lobby_dict.get("MaxPlayers", 0)
		
		var btn = Button.new()
		btn.text = "%s (%d/%d) %s" % [lobby_name, player_count, max_players, lobby_map]
		btn.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		# Store the lobby ID in the button's metadata
		btn.name = str(lobby_id)
		btn.pressed.connect(_on_lobby_button_pressed.bind(btn))
		lobby_list.add_child(btn)
	
	# Reset selection
	selected_lobby_id = -1
	join_btn.disabled = true

func _add_no_lobbies_label():
	var label = Label.new()
	label.text = "No lobbies available"
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lobby_list.add_child(label)

# Called when a lobby button is clicked
func _on_lobby_button_pressed(btn):
	# Toggle selection: if the same button is clicked again, deselect
	if selected_lobby_id == int(btn.name):
		selected_lobby_id = -1
		join_btn.disabled = true
		# Optionally reset button style
		btn.add_theme_color_override("font_color", Color.WHITE)
		return
	
	# Clear previous selection style
	for child in lobby_list.get_children():
		if child is Button:
			child.add_theme_color_override("font_color", Color.WHITE)
	
	# Highlight selected button
	btn.add_theme_color_override("font_color", Color.YELLOW)
	selected_lobby_id = int(btn.name)
	join_btn.disabled = false

# Refresh button pressed
func _on_refresh_pressed():
	_refresh_lobby_list()

func _on_create_pressed():
	main_node.rpc_id(1, "CreateLobby")
	_refresh_lobby_list()

# Join button pressed – calls the C# method 
func _on_join_pressed():
	if selected_lobby_id < 0:
		return
	
	#main_node.ClearLobby()
	main_node.rpc_id(1, "ConnectRoom",  multiplayer.get_unique_id(), selected_lobby_id )
	print("Joining lobby ID: ", selected_lobby_id)
	visible = false
