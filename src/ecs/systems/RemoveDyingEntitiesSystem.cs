using Ecs;
using System.Collections.Generic;
using System.Linq;

public class RemoveDyingEntitiesSystem : Ecs.System
{
    private const string AffiliatedEntitiesKey = "affiliated";

    public RemoveDyingEntitiesSystem()
    {
        AddRequiredComponent<Dying>();
        AddRequiredComponent<ProfileDetails>(AffiliatedEntitiesKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        if (manager.CurrentState is CombatEndState)
        {
            return;
        }

        base.UpdateAll(deltaTime);

        var affCounts = new Dictionary<Affiliation, int>() { { Affiliation.Friendly, 0 }, { Affiliation.Enemy, 0 }, { Affiliation.Neutral, 0 } };
        EntitiesFor(AffiliatedEntitiesKey).ForEach(entity =>
        {
            affCounts[entity.GetComponent<ProfileDetails>().Affiliation]++;
        });
        if (affCounts[Affiliation.Friendly] == 0)
        {
            manager.ApplyState(new CombatEndState(EndCondition.Lose));
        }
        else if (affCounts[Affiliation.Enemy] == 0)
        {
            manager.ApplyState(new CombatEndState(EndCondition.Win));
        }
    }

    protected override void Update(Entity entity, float deltaTime) 
    {
        entity.GetComponentOrNull<TurnOrderCard>()?.Cleanup();

        manager.DeleteEntity(entity.Id);
    }
}