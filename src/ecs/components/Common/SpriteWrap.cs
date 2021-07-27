using Ecs;
using Godot;

public class SpriteWrap : WrapComponent<Sprite>
{
    public Sprite Sprite
    {
        get => wrappedNode;
        private set => wrappedNode = value;
    }
}
