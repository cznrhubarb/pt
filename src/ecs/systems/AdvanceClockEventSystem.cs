using Ecs;
using System.Linq;

public class AdvanceClockEventSystem : Ecs.System
{
    private const string TurnSpeedEntityKey = "turnSpeed";

    public AdvanceClockEventSystem()
    {
        AddRequiredComponent<AdvanceClockEvent>();
        AddRequiredComponent<TurnSpeed>(TurnSpeedEntityKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        if (EntitiesFor(PrimaryEntityKey).Count != 0)
        {
            var advanceSpeeds = EntitiesFor(TurnSpeedEntityKey)
                .Select(ent => ent.GetComponent<TurnSpeed>());

            var amountToAdvance = advanceSpeeds.Min(speedComp => speedComp.TimeToAct);

            foreach (var speedComp in advanceSpeeds)
            {
                speedComp.TimeToAct -= amountToAdvance;
            }

            // TODO: This should be handled better so that if anyone else needs this event, they also get it
            foreach (var evt in EntitiesFor(PrimaryEntityKey))
            {
                manager.DeleteEntity(evt.Id);
            }
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
