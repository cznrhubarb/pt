using System.Collections.Generic;

public static class WorldState
{
    public static List<MonsterState> PartyState { get; set; } = new List<MonsterState>()
    {
        MonsterFactory.BuildMonster(DataLoader.BlueprintData[150], 1)
    };

    public static List<MonsterState> RivalPartyState { get; set; } = new List<MonsterState>()
    {
        MonsterFactory.BuildMonster(DataLoader.BlueprintData[149], 1)
    };

    public static List<Item> Inventory { get; set; } = new List<Item>();

    public static List<MonsterState> MonsterBank { get; set; } = new List<MonsterState>();
}
