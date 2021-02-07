using Godot;

public class SpriteWrap : Ecs.WrapComponent<Sprite>
{
    public Sprite Sprite
    {
        get => wrappedNode;
        private set => wrappedNode = value;
    }
}
