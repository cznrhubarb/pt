using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(SpriteWrap), "res://editoricons/Component.svg", nameof(Resource))]
public class SpriteWrap : WrapComponent<Sprite>
{
    public Sprite Sprite
    {
        get => wrappedNode;
        private set => wrappedNode = value;
    }
}
