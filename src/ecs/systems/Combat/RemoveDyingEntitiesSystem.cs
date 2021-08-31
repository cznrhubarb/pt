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

        // TODO: Other victory/loss conditions can be added via additional systems.
        //  Might need to disable this win condition if there are any combats that don't have a "kill everyone" victory condition
        // TODO: Need to add a "trainer loss" condition here
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
