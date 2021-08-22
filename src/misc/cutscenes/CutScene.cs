using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CutScene), "res://editoricons/CutScene.svg", nameof(Resource))]
public class CutScene : Resource
{
    [Export]
    public CutSceneEvent[] Events { get; set; } = new CutSceneEvent[0];

    public CutScene() { }
}
