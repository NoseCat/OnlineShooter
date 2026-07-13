using Godot;
using System.Collections.Generic;

public partial class Main
{
    private readonly Dictionary<long, Node> playersGlobal = new(); //global list of players

    [Export]
    public Godot.Collections.Array<Godot.Collections.Dictionary> LobbyData { get; set; } = new();
    private int nextLobbyId = 0;

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
        CreateLobby();
        CreateLobby();
        CreateLobby();
        CreateLobby();
    }

    //us this to get proper id from LobbyData, otherwise we cant be certain of prper id
    //LobbyData[GetLobbyIndexById(id)]  what are we even doing 
    private int GetLobbyIndexById(int id)
    {
        for (int i = 0; i < LobbyData.Count; i++)
        {
            if (LobbyData[i]["Id"].AsInt32() == id)
                return i;
        }
        return -1;
    }

    private void CreateLobby()
    {
        var lobbyDict = new Godot.Collections.Dictionary
        {
            ["Id"] = nextLobbyId,
            ["Name"] = "Room_" + nextLobbyId,
            ["MaxPlayers"] = 5,
            ["Map"] = new[] { "mp_2_forts", "mp_tower" }[GD.Randi() % 2],
            ["Players"] = new Godot.Collections.Array<int>()
        };
        LobbyData.Add(lobbyDict);
        var room = AddChildLobby(nextLobbyId);
        nextLobbyId++;

        Logger.Log("Server", $"Lobby created.");
    }

    //
    private Node AddChildLobby(int lobby_id)
    {
        PackedScene room = ResourceLoader.Load<PackedScene>("res://game_objects/room.tscn");
        Node roomInstance = room.Instantiate();
        AddChild(roomInstance);

        roomInstance.Name = LobbyData[GetLobbyIndexById(lobby_id)]["Name"].ToString();
        roomInstance.Call("load_map", LobbyData[GetLobbyIndexById(lobby_id)]["Map"].ToString());

        //var roomMultiplayer = new SceneMultiplayer();
        //roomMultiplayer.MultiplayerPeer = Multiplayer.MultiplayerPeer;
        //GetTree().SetMultiplayer(roomMultiplayer, roomInstance.GetPath());

        var PlayerSpawner = roomInstance.GetNode<MultiplayerSpawner>("MultiplayerSpawner");
        PlayerSpawner.SpawnFunction = new Callable(this, nameof(SpawnPlayerFunc));
        PlayerSpawner.SpawnPath = roomInstance.GetPath() + "/PlayerContainer";
        return roomInstance;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private async void SpawnLobby(int player_id, int lobby_id)
    {
        await ToSignal(LobbySynchronizer, "synchronized");
        var room = AddChildLobby(lobby_id);
        var players = LobbyData[GetLobbyIndexById(lobby_id)]["Players"].AsGodotArray<int>();
        foreach (int player in players)
        {
            if(player == player_id)
                continue;
            var spawnData = new Godot.Collections.Dictionary
            {
                ["player_id"] = player,
                ["lobby_id"] = lobby_id
            };
            var playerobj = SpawnPlayerFunc(spawnData);
            playerobj.SetMultiplayerAuthority(player);
            room.GetNode<Node3D>("PlayerContainer").AddChild(playerobj);
        }
    }

    private Node SpawnPlayer(int player_id, int lobby_id)
    {
        var PlayerSpawner = GetNode<MultiplayerSpawner>($"./{LobbyData[GetLobbyIndexById(lobby_id)]["Name"]}/MultiplayerSpawner");
        var spawnData = new Godot.Collections.Dictionary
        {
            ["player_id"] = player_id,
            ["lobby_id"] = lobby_id
        };

        return PlayerSpawner.Spawn(spawnData);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private async void ConnectRoom(int player_id, int lobby_id)
    {
        GD.Print("Enter Room");
        var lobbyDict = LobbyData[GetLobbyIndexById(lobby_id)];
        var playersArray = lobbyDict["Players"].AsGodotArray<int>();
        playersArray.Add(player_id);
        LobbyData[GetLobbyIndexById(lobby_id)] = lobbyDict; //to force sync on nested update

        RpcId(player_id, "SpawnLobby", player_id, lobby_id);
        for (int i = 0; i < 30; i++)
            await ToSignal(GetTree(), "process_frame"); // I wish I knew what are we waiting for
        foreach(Node playerObj in playersGlobal.Values)
        {
            playerObj.Rpc("update_visibility");
        }
        var player = SpawnPlayer(player_id, lobby_id);
        playersGlobal[player_id] = player;
        player.Rpc("update_visibility");
        player.Rpc("set_authority", player_id);
        player.RpcId(player_id, "set_up");

    }

    //server does this when peer connects
    private void OnPeerConnected(long id)
    {
        Logger.Log("Server", $"Peer connected: {id}");
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Log("Server", $"Peer disconnected: {id} (did nothing)");
        //players[id].QueueFree(); // for clients deletion is handled by MultiplayerSpawner
        //players.Remove(id);
    }
}