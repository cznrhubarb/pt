using Ecs;
using Godot;
using System.Linq;

public class RenderTargetProfileSystem : Ecs.System
{
    private const string PotentialTargetKey = "potentialTarget";
    private const string TargetedKey = "targeted";

    public RenderTargetProfileSystem()
    {
        AddRequiredComponent<TargetIndicator>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TileLocation>(PotentialTargetKey);
        AddRequiredComponent<ProfileDetails>(PotentialTargetKey);
        AddRequiredComponent<Health>(PotentialTargetKey);
        AddRequiredComponent<Targeted>(TargetedKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        var currentTargets = EntitiesFor(TargetedKey);
        foreach (var target in currentTargets)
        {
            manager.RemoveComponentFromEntity<Targeted>(target);
        }

        var indicators = EntitiesFor(PrimaryEntityKey);
        var potentialTargets = EntitiesFor(PotentialTargetKey);

        var actualTargets = potentialTargets.Where(target => 
            indicators.Any(ind => 
                target.GetComponent<TileLocation>().TilePosition == ind.GetComponent<TileLocation>().TilePosition &&
                ind.GetComponent<SpriteWrap>().Sprite.Visible
            )
        );

        foreach (var target in actualTargets)
        {
            manager.AddComponentToEntity(target, new Targeted());
        }

        // TODO: The actual rendering should probably happen in a system other than the one that sets the targets
        //  There we can also do accuracy and damage calcs to display
        manager.PerformHudAction("SetProfile", Direction.Right, actualTargets.FirstOrDefault());
        // TODO: Indicate if there are more than one
        // TODO: Deterministic sort by distance from center
        // TODO: Maybe here, maybe somewhere else, but display an on map indicator of which unit is displaying profile card
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
