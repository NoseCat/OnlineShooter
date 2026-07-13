using Godot;

public partial class Main
{
    private void Client()
    {
        GD.Print($"Client mode. Connecting to {Address}:{Port}");
        var peer = new ENetMultiplayerPeer();

        Error error = peer.CreateClient(Address, Port);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Connection failed: {error}");
            return;
        }


        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Connected!");

    }

    private async void DisconnectClient(int lobby_id)
    {
        RpcId(1, "DisconnectRoom", Multiplayer.GetUniqueId());
        //for (int i = 0; i < 30; i++)
            //await ToSignal(GetTree(), "process_frame");
        GetNode<Control>("MainMenu").Visible = true;
        ClearLobby();
    }

    private void ClearLobby()
    {
        foreach (var lobby_dict in LobbyData)
        {
            GD.Print(lobby_dict["Name"].ToString());
            var room = GetNodeOrNull<Node>(lobby_dict["Name"].ToString());
            if(room != null)
            {
                room.GetNode<MultiplayerSpawner>("MultiplayerSpawner").QueueFree();
                room.QueueFree();
            }
        }
    }
}