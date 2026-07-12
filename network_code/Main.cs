using Godot;
using System;
using System.Linq;

public partial class Main : Node
{
	private const int Port = 4433;
	private const string Address = "127.0.0.1";

	//MultiplayerSpawner PlayerSpawner;

	MultiplayerSynchronizer LobbySynchronizer;

	public override void _Ready()
	{
		//sync lobby info across all peers
		LobbySynchronizer = GetNode<MultiplayerSynchronizer>("LobbySynchronizer");
		LobbySynchronizer.RootPath = "..";
		SceneReplicationConfig ReplicationConfig = new();
		ReplicationConfig.AddProperty(":LobbyData");
		LobbySynchronizer.ReplicationConfig = ReplicationConfig;

		if (OS.GetCmdlineArgs().Contains("--server"))
			Server();
		else
			Client();
	}
	public Node SpawnPlayerFunc(Godot.Collections.Dictionary data)
	{
		int id = (int)data["player_id"];
		var playerScene = ResourceLoader.Load<PackedScene>("res://game_objects/player.tscn");
		var player = playerScene.Instantiate<CharacterBody3D>();
		player.Name = id.ToString();
		//player.SetMultiplayerAuthority(id);
		player.Set("lobby_id", data["lobby_id"]);
		return player;
	}

}
