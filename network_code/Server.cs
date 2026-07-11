using Godot;
using System.Collections.Generic;

public partial class Main
{
    private readonly Dictionary<long, Node> players = new(); //global list of players

    [Export]
    public Godot.Collections.Array<Lobby> Lobbies { get; set; } = new();
    private int nextLobbyId = 1;

    private void Server()
    {
        Logger.Log("Server", "Starting server...");
        var peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(Port);
        if (error != Error.Ok)
        {
            Logger.Log("Error", $"Failed to start server: {error}");
            return;
        }
        Multiplayer.MultiplayerPeer = peer;
        Logger.Log("Server", $"Server started on port {Port}");

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;

        CreateLobby();
        //SpawnLobby(0);
    }

    private void CreateLobby()
    {
        var lobby = new Lobby{Id = 0, Name = "Room " + 0, MaxPlayers = 5, Map = "mp_2_forts"};
        Lobbies.Add(lobby);
        
        Lobbies = Lobbies; //a hack, needed because sycnronizer will only replicate on setters, allegedly 
        Logger.Log("Server", $"Lobby created.");
    }

    //load static things about map
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)] 
    private void SpawnLobby(int id)
    {
        //var lobby = lobbies[id];
        PackedScene room = ResourceLoader.Load<PackedScene>("res://game_objects/room.tscn");
        Node roomInstance = room.Instantiate();
        //roomInstance.LoadMap
        AddChild(roomInstance);
    }

    //figure out what needs to be sent (visibility, spawning, etc)
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] 
    private void ConnectRoom(int lobby_id, int player_id)
    {
        GD.Print("Enter Room");
    }

    //server does this when peer connects
    private void OnPeerConnected(long id)
    {
        Logger.Log("Server", $"Peer connected: {id}");
        Node player = PlayerSpawner.Spawn(id); // for now just spawn him
        players[id] = player;

        ConnectRoom(0, (int)id);
        RpcId(id, "SpawnLobby", 0);
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Log("Server", $"Peer disconnected: {id}");
        players[id].QueueFree(); // for clients deletion is handled by MultiplayerSpawner
        players.Remove(id);
    }
}