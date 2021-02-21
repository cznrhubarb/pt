using Godot;

public class Globals : Node
{
    private static SceneTree sceneTree;
    public static SceneTree SceneTree { get => sceneTree; }

    public override void _Ready()
    {
        sceneTree = GetTree();
    }
}
