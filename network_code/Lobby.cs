using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Lobby : GodotObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MaxPlayers { get; set; }
    public string Map {get; set;}
    public Dictionary<int, Node> Players { get; set; } = new();
}