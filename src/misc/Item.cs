using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(Item), "res://editoricons/Component.svg", nameof(Resource))]
public class Item
{
    [Export]
    public string Name { get; set; } = "";

    [Export]
    public Texture Sprite { get; set; } = null;
}