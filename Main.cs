using Godot;
using System;
using System.Linq;
using System.Net.WebSockets;

public partial class Main : Node3D
{
	private const int Port = 4433;
	private const string address = "127.0.0.1"; 

	private MultiplayerSpawner spawner;
	private int peers = 0; 

	public override void _Ready()
	{
		spawner = GetNode<MultiplayerSpawner>("MultiplayerSpawner");
		spawner.SpawnFunction = new Callable(this, nameof(CustomSpawn));
		if (OS.GetCmdlineArgs().Contains("--server"))
		{
			Server();
		}
		else
		{
			Client(address);
		}
	}

	// Server
	private void Server()
	{
		GD.Print("Starting server...");
		var peer = new ENetMultiplayerPeer();
		
		Error error = peer.CreateServer(Port);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to start server: {error}");
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		GD.Print($"Server started on port {Port}");

		// Listen for connections
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
	}

	public Node CustomSpawn(Variant data)
	{
		int id = (int)data;
		var playerScene = ResourceLoader.Load<PackedScene>("res://game_objects//player.tscn");
		var player = playerScene.Instantiate<CharacterBody3D>();
		player.Name = id.ToString();
		//player.Call("set_id", (int)id);
		player.SetMultiplayerAuthority(id, true); //who is in controll of rpc methods for this node

		// Spawn at random positions, spread out to avoid physics overlap
		//GetNode<Node>("PlayerContainer").AddChild(player, true);
		//player.GlobalPosition += new Vector3(10, 0, 10) * peers;
		return player;
	}

	public void OnPeerConnected(long id)
	{
		GD.Print($"peer connceted: {id}");
		peers++;

		spawner.Spawn(id);
	}

	public void OnPeerDisconnected(long id)
	{
		
	}

	// Client 
	public void Client(string address = "127.0.0.1")
	{
		GD.Print($"Client mode. Connecting to {address}:{Port}");
		var peer = new ENetMultiplayerPeer();

		Error error = peer.CreateClient(address, Port);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Connection failed: {error}");
			return;
		}

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Connected!");
	}
}
