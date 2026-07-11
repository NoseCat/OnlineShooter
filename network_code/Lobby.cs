using Godot;
using Godot.Collections; // for Array<T>

[GlobalClass]
public partial class Lobby : Resource
{
    [Export] public int Id { get; set; }
    [Export] public string Name { get; set; } = "";
    [Export] public int MaxPlayers { get; set; }
    [Export] public string Map { get; set; } = "";
    [Export] public Array<int> Players { get; set; } = new();
}