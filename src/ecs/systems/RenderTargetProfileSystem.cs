using Ecs;
using Godot;
using System.Linq;

public class RenderTargetProfileSystem : Ecs.System
{
    private const string PotentialTargetKey = "potentialTarget";

    public RenderTargetProfileSystem()
    {
        AddRequiredComponent<TargetIndicator>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TileLocation>(PotentialTargetKey);
        AddRequiredComponent<ProfileDetails>(PotentialTargetKey);
        AddRequiredComponent<Health>(PotentialTargetKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        var indicators = EntitiesFor(PrimaryEntityKey);
        var potentialTargets = EntitiesFor(PotentialTargetKey);

        var actualTargets = potentialTargets.Where(target => 
            indicators.Any(ind => 
                target.GetComponent<TileLocation>().TilePosition == ind.GetComponent<TileLocation>().TilePosition &&
                ind.GetComponent<SpriteWrap>().Sprite.Visible
            )
        );

        (manager as Combat).SetProfile(Direction.Right, actualTargets.FirstOrDefault());
        // TODO: Indicate if there are more than one
        // TODO: Deterministic sort by distance from center
        // TODO: Maybe here, maybe somewhere else, but display an on map indicator of which unit is displaying profile card
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
