using Godot;
using System;
using System.Linq;

public partial class Main : Node3D
{
	private MultiplayerSpawner spawner;
	private const int Port = 4433;
	private const string Address = "127.0.0.1";

	public override void _Ready()
	{
		spawner = GetNode<MultiplayerSpawner>("MultiplayerSpawner");
		spawner.SpawnFunction = new Callable(this, nameof(CustomSpawn));

		if (OS.GetCmdlineArgs().Contains("--server"))
			Server();
		else
			Client();

		
	}
	public Node CustomSpawn(Variant data)
	{
		int id = (int)data;
		var playerScene = ResourceLoader.Load<PackedScene>("res://game_objects/player.tscn");
		var player = playerScene.Instantiate<CharacterBody3D>();
		player.Name = id.ToString();
		player.SetMultiplayerAuthority(id, true);
		return player;
	}

}
