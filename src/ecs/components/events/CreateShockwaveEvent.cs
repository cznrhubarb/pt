using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(CreateShockwaveEvent), "res://editoricons/Component.svg", nameof(Resource))]
public class CreateShockwaveEvent : Component
{
    public Vector3 InitialTilePosition { get; set; }

    public Vector3 Direction { get; set; }

    public float Magnitude { get; set; }

    // Potentially would want transfer %? Going to make it static for now.
}
