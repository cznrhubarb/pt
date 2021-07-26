using Godot;
using System;

public class Globals : Node
{
    public static SceneTree SceneTree { get; private set; }

    public static Random Random { get; private set; } = new Random();

    public override void _Ready()
    {
        SceneTree = GetTree();
    }
}