using Godot;
using System;
using System.Collections.Generic;

public class Globals : Node
{
    public static SceneTree SceneTree { get; private set; }

    public static Random Random { get; private set; } = new Random();

    public static Dictionary<string, Vector2> cameraAnchorOffsets = new Dictionary<string, Vector2>();

    public override void _Ready()
    {
        DataLoader.LoadAll();
        SceneTree = GetTree();
    } 
}
