using Ecs;
using System.Linq;

public class AdvanceClockEventSystem : Ecs.System
{
    private const string TurnSpeedEntityKey = "turnSpeed";
    private const string MapEntityKey = "map";

    public AdvanceClockEventSystem()
    {
        AddRequiredComponent<AdvanceClockEvent>();
        AddRequiredComponent<TurnSpeed>(TurnSpeedEntityKey);
        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var advanceEntities = EntitiesFor(TurnSpeedEntityKey)
            .Where(ent => !ent.HasComponent<Dying>())
            .Select(ent => new SortPair() { entity = ent, turnSpeed = ent.GetComponent<TurnSpeed>() })
            .OrderBy(sp => sp.turnSpeed.TimeToAct);

        var firstAdvanceEntity = advanceEntities.First();
        var amountToAdvance = firstAdvanceEntity.turnSpeed.TimeToAct;

        foreach (var pair in advanceEntities)
        {
            pair.turnSpeed.TimeToAct -= amountToAdvance;
        }

        var nextActor = firstAdvanceEntity.entity;
        if (nextActor?.GetComponentOrNull<PlayerCharacter>() != null)
        {
            manager.ApplyState(new PlayerTurnStartState(nextActor, SingleEntityFor(MapEntityKey)));
            //manager.ApplyState(new PlayerMovementState(nextActor, SingleEntityFor(MapEntityKey)));
        }
        else if (nextActor?.GetComponentOrNull<FriendlyNpc>() != null ||
            nextActor?.GetComponentOrNull<EnemyNpc>() != null)
        {
            manager.ApplyState(new NpcPlanningState(nextActor, SingleEntityFor(MapEntityKey)));
        }

        manager.DeleteEntity(entity.Id);
    }

    private struct SortPair
    {
        public TurnSpeed turnSpeed;
        public Entity entity;
    }
}
