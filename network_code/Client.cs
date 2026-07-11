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
        
        //ConnectRoom(0);
    }


}