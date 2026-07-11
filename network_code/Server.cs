using Godot;
using System.Collections.Generic;

public partial class Main
{
    //private readonly Dictionary<long, Node> players = new(); //global list of players

    [Export]
    public Godot.Collections.Array<Lobby> LobbyInfo { get; set; } = new();
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

    private Lobby CreateLobby()
    {
        var lobby = new Lobby{Id = 0, Name = "Room_0", MaxPlayers = 5, Map = "mp_2_forts"};
        LobbyInfo.Add(lobby);
        
        var room = AddChildLobby(LobbyInfo[0].Name);

        LobbyInfo = LobbyInfo; //a hack, needed because sycnronizer will only replicate on setters, allegedly 
        Logger.Log("Server", $"Lobby created.");
        return lobby;
    }

    private Node AddChildLobby(string name)
    {
        PackedScene room = ResourceLoader.Load<PackedScene>("res://game_objects/room.tscn");
        Node roomInstance = room.Instantiate();
        //roomInstance.LoadMap
        AddChild(roomInstance);
        roomInstance.Name = name;
        var PlayerSpawner = roomInstance.GetNode<MultiplayerSpawner>("MultiplayerSpawner");
		PlayerSpawner.SpawnFunction = new Callable(this, nameof(SpawnPlayer));
        return roomInstance;
    }

    //load static things about map, this assumes we are calling this locally
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)] 
    private Node SpawnLobby(int player_id, string name)
    {
        var room = AddChildLobby(name);
        return room;
        
    }

    //figure out what needs to be sent (visibility, spawning, etc)
    private async void ConnectRoom(int player_id, int lobby_id)
    {
        GD.Print("Enter Room");

        LobbyInfo[lobby_id].Players.Add(player_id);
        RpcId(player_id, "SpawnLobby", player_id, LobbyInfo[lobby_id].Name);

        await ToSignal(GetTree(), "process_frame"); //triple hack!
        await ToSignal(GetTree(), "process_frame");
        await ToSignal(GetTree(), "process_frame");

        var PlayerSpawner = GetNode<MultiplayerSpawner>($"{LobbyInfo[lobby_id].Name}/MultiplayerSpawner");
        Node player = PlayerSpawner.Spawn(player_id);
    }

    //server does this when peer connects
    private void OnPeerConnected(long id)
    {
        Logger.Log("Server", $"Peer connected: {id}");

        ConnectRoom((int)id, 0);
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Log("Server", $"Peer disconnected: {id} (did nothing)");
        //players[id].QueueFree(); // for clients deletion is handled by MultiplayerSpawner
        //players.Remove(id);
    }
}