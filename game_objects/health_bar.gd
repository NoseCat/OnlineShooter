extends Node

@onready var player = $".."


func _ready() -> void:
	pass # Replace with function body.


func _process(_delta: float) -> void:
	var healthUV = player.health / player.max_health

	
	$Health3d.mesh.size.x = healthUV * 1.0
	$Health3d/Label3D.text = str(player.health)
	if(not is_multiplayer_authority()):
		$Control.visible = false;
		return
	$Control/ColorRect.size.x = healthUV * get_viewport().size.x / 4
	$Control/ColorRect.position.x = -$Control/ColorRect.size.x/2 + 20
	$Control/Label.text = str(player.health)
	$Health3d.visible = false
