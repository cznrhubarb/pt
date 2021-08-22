using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(FightStats), "res://editoricons/Component.svg", nameof(Resource))]
public class FightStats : Component
{
    public int Str { get; set; }
    public int Tuf { get; set; }
    public int Dex { get; set; }
    public int Mag { get; set; }
    public int Atn { get; set; }
}
