using Ecs;
using Godot;
using MonoCustomResourceRegistry;
using System.Collections.Generic;

[RegisteredType(nameof(CombatSpoils), "res://editoricons/Component.svg", nameof(Resource))]
public class CombatSpoils : Component
{
    public Dictionary<Affiliation, List<MonsterState>> DeathPool { get; set; } =
        new Dictionary<Affiliation, List<MonsterState>>() {
            { Affiliation.Friendly, new List<MonsterState>() },
            { Affiliation.Enemy, new List<MonsterState>() },
            { Affiliation.Neutral, new List<MonsterState>() }
        };

    public List<Item> FoundItems { get; set; } = new List<Item>();
}
