using Godot;
using System.Collections.Generic;

public partial class Main
{
    private readonly Dictionary<long, Node> players = new();

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
    }

    private void OnPeerConnected(long id)
    {
        Logger.Log("Server", $"Peer connected: {id}");
        Node player = spawner.Spawn(id);
        if (player != null)
            players[id] = player;
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Log("Server", $"Peer disconnected: {id}");
        players[id].QueueFree(); // for clients delition is handled by MultiplayerSpawner
        players.Remove(id);
    }
}