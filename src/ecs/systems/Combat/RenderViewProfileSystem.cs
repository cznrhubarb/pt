using Ecs;
using System.Linq;

public class RenderViewProfileSystem : Ecs.System
{
    private const string PotentialTargetKey = "potentialTarget";

    public RenderViewProfileSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();

        AddRequiredComponent<TileLocation>(PotentialTargetKey);
        AddRequiredComponent<ProfileDetails>(PotentialTargetKey);
        AddRequiredComponent<Health>(PotentialTargetKey);
        AddRequiredComponent<FightStats>(PotentialTargetKey);
    }
    public override void UpdateAll(float deltaTime)
    {
        // A bit of a hack, but we want this system to run in every state
        //  EXCEPT for player targeting (it has a different profile view system)
        if (!(manager.CurrentState is PlayerTargetingState))
        {
            base.UpdateAll(deltaTime);
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var reticlePos = entity.GetComponent<TileLocation>().TilePosition;
        foreach (var potential in EntitiesFor(PotentialTargetKey))
        {
            if (potential.GetComponent<TileLocation>().TilePosition == reticlePos)
            {
                manager.PerformHudAction("SetProfile", Direction.Right, potential);
                return;
            }
        }

        manager.PerformHudAction("SetProfile", Direction.Right, null);
    }
}
