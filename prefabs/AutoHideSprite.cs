using Godot;

public class AutoHideSprite : Sprite
{
    // Designed to just be a placeholder sprite for editor
    public override void _Ready()
    {
        QueueFree();
    }
}
