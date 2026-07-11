using Godot;
using System.Collections.Generic;

public partial class Main
{
    //private readonly Dictionary<long, Node> players = new(); //global list of players

    //[Export]
    //public Godot.Collections.Array<Lobby> LobbyInfo { get; set; } = new();
    [Export]
    public Godot.Collections.Array<Godot.Collections.Dictionary> LobbyData { get; set; } = new();
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
    }

    private void CreateLobby()
    {
        var lobbyDict = new Godot.Collections.Dictionary
        {
            //["Id"] = 0,
            ["Name"] = "Room_0",
            ["MaxPlayers"] = 5,
            ["Map"] = new[] { "mp_2_forts", "mp_tower" }[GD.Randi() % 2],
            ["Players"] = new Godot.Collections.Array<int>()  // use Array, not List
        };
        LobbyData.Add(lobbyDict);

        var room = AddChildLobby(0);

        Logger.Log("Server", $"Lobby created.");
    }

    //
    private Node AddChildLobby(int lobby_id)
    {
        PackedScene room = ResourceLoader.Load<PackedScene>("res://game_objects/room.tscn");
        Node roomInstance = room.Instantiate();
        AddChild(roomInstance);

        roomInstance.Name = LobbyData[lobby_id]["Name"].ToString();
        roomInstance.Call("load_map", LobbyData[lobby_id]["Map"].ToString());

        var PlayerSpawner = roomInstance.GetNode<MultiplayerSpawner>("MultiplayerSpawner");
        PlayerSpawner.SpawnFunction = new Callable(this, nameof(SpawnPlayer));
        return roomInstance;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private async void SpawnLobby(int player_id, int lobby_id)
    {
        await ToSignal(LobbySynchronizer, "synchronized");
        var room = AddChildLobby(lobby_id);
        RpcId(1, "SpawnPlayer", player_id, lobby_id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void SpawnPlayer(int player_id, int lobby_id)
    {
        var PlayerSpawner = GetNode<MultiplayerSpawner>($"{LobbyData[lobby_id]["Name"]}/MultiplayerSpawner");
        Node player = PlayerSpawner.Spawn(player_id);
    }

    private void ConnectRoom(int player_id, int lobby_id)
    {

        GD.Print("Enter Room");
        var lobbyDict = LobbyData[lobby_id];
        var playersArray = lobbyDict["Players"].AsGodotArray<int>();
        playersArray.Add(player_id);
        LobbyData[lobby_id] = lobbyDict; //to force sync on nested update

        RpcId(player_id, "SpawnLobby", player_id, lobby_id);

        //await ToSignal(GetTree(), "process_frame"); //triple hack!

    }

    //server does this when peer connects
    private void OnPeerConnected(long id)
    {
        Logger.Log("Server", $"Peer connected: {id}");

        //ConnectRoom((int)id, 0);
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Log("Server", $"Peer disconnected: {id} (did nothing)");
        //players[id].QueueFree(); // for clients deletion is handled by MultiplayerSpawner
        //players.Remove(id);
    }
}