using Godot;

static class GDExtensions
{
    public static Vector2 Flatten(this Vector3 vec3)
    {
        return new Vector2(vec3.x, vec3.y);
    }
}
