using Godot;
using System;

public class Target : Sprite
{
    const float SquishSpeed = 3;
    const float SquishAmount = 0.03f;

    public float squishAccumulator = 0;

    public override void _Process(float delta)
    {
        squishAccumulator += delta * SquishSpeed;
        float squish = 1 + ((float)Math.Sin(squishAccumulator) * SquishAmount);
        this.Scale = new Vector2(squish, squish);
    }
}
