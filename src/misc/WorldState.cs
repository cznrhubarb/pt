using Godot;
using System.Collections.Generic;

public static class WorldState
{
    public static List<MonsterState> PartyState { get; set; } = new List<MonsterState>()
    {
        MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Trainer.tres"), 1)
    };

    public static List<MonsterState> RivalPartyState { get; set; } = new List<MonsterState>()
    {
        MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/WaTrainer.tres"), 1)
    };

    public static List<Item> Inventory { get; set; } = new List<Item>();

    public static List<MonsterState> MonsterBank { get; set; } = new List<MonsterState>();
}
