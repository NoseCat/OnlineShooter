using Godot;
using System.Collections.Generic;

public partial class Main
{
    private readonly Dictionary<long, Node> players = new();

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

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Peer connected: {id}");
        Node player = spawner.Spawn(id);
        if (player != null)
            players[id] = player;
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Peer disconnected: {id} (did nothing about it)");
    }
}