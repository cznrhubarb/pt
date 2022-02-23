
using Godot;
using System;

public enum Direction
{
    Down,
    Right,
    Up,
    Left,
}

static class DirectionExtensions
{
    public static Vector3 ToVector3(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return new Vector3(0, -1, 0);
            case Direction.Right: return new Vector3(1, 0, 0);
            case Direction.Down: return new Vector3(0, 1, 0);
            case Direction.Left: return new Vector3(-1, 0, 0);
            default: throw new ArgumentOutOfRangeException("direction");
        }
    }

    public static Direction ToDirection(this Vector3 vector)
    {
        if (Mathf.Abs(vector.x) >= Mathf.Abs(vector.y))
        {
            vector.y = 0;
        }
        else
        {
            vector.x = 0;
        }

        var flatVec = new Vector2(vector.x, vector.y).Normalized();
        if (flatVec == Vector2.Up)
            return Direction.Up;
        else if (flatVec == Vector2.Down)
            return Direction.Down;
        else if (flatVec == Vector2.Left)
            return Direction.Left;
        else if (flatVec == Vector2.Right)
            return Direction.Right;
        else
            throw new ArgumentOutOfRangeException("direction");
    }
}