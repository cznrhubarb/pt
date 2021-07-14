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

    // TODO: I think I can just use Update instead of having to override UpdateAll
    public override void UpdateAll(float deltaTime)
    {
        if (EntitiesFor(PrimaryEntityKey).Count != 0)
        {
            var advanceEntities = EntitiesFor(TurnSpeedEntityKey)
                .Select(ent => new SortPair() { entity = ent, turnSpeed = ent.GetComponent<TurnSpeed>() })
                .OrderBy(sp => sp.turnSpeed.TimeToAct);

            var firstAdvanceEntity = advanceEntities.First();
            var amountToAdvance = firstAdvanceEntity.turnSpeed.TimeToAct;

            foreach (var pair in advanceEntities)
            {
                pair.turnSpeed.TimeToAct -= amountToAdvance;
            }
            // TODO: If we want to make speed determined by player action taken, this should move elsewhere
            //      Could be good if the moves themselves had speed as an attribute?
            firstAdvanceEntity.turnSpeed.TimeToAct = firstAdvanceEntity.turnSpeed.Speed;

            var nextActor = firstAdvanceEntity.entity;
            if (nextActor?.GetComponentOrNull<PlayerCharacter>() != null)
            {
                manager.ApplyState(new PlayerMovementState() { Acting = nextActor, Map = SingleEntityFor(MapEntityKey) });
            }
            else if (nextActor?.GetComponentOrNull<FriendlyNpc>() != null ||
                nextActor?.GetComponentOrNull<EnemyNpc>() != null)
            {
                manager.ApplyState(new NpcTurnState() { Acting = nextActor });
            }

            // TODO: This should be handled better so that if anyone else needs this event, they also get it
            foreach (var evt in EntitiesFor(PrimaryEntityKey))
            {
                manager.DeleteEntity(evt.Id);
            }
        }
    }

    protected override void Update(Entity entity, float deltaTime) { }

    private struct SortPair
    {
        public TurnSpeed turnSpeed;
        public Entity entity;
    }
}
