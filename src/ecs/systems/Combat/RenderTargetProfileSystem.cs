using Ecs;
using System.Linq;

public class RenderTargetProfileSystem : Ecs.System
{
    private const string PotentialTargetKey = "potentialTarget";
    private const string TargetedKey = "targeted";
    private const string SelectedKey = "selected";
    private const string ReticleKey = "reticle";
    private const string MapKey = "map";

    public RenderTargetProfileSystem()
    {
        AddRequiredComponent<TargetIndicator>();
        AddRequiredComponent<TileLocation>();

        AddRequiredComponent<Selected>(SelectedKey);
        AddRequiredComponent<TileLocation>(SelectedKey);
        AddRequiredComponent<FightStats>(SelectedKey);

        AddRequiredComponent<TileLocation>(PotentialTargetKey);
        AddRequiredComponent<ProfileDetails>(PotentialTargetKey);
        AddRequiredComponent<FightStats>(PotentialTargetKey);
        AddRequiredComponent<Health>(PotentialTargetKey);

        AddRequiredComponent<Reticle>(ReticleKey);
        AddRequiredComponent<TileLocation>(ReticleKey);

        AddRequiredComponent<Targeted>(TargetedKey);

        AddRequiredComponent<Map>(MapKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        var currentTargets = EntitiesFor(TargetedKey);
        foreach (var target in currentTargets)
        {
            manager.RemoveComponentFromEntity<Targeted>(target);
        }

        if (manager.CurrentState is PlayerTargetingState ptState)
        {
            var indicatorLocations = EntitiesFor(PrimaryEntityKey)
                .Where(ind => ind.GetComponent<SpriteWrap>().Sprite.Visible)
                .Select(ind => ind.GetComponent<TileLocation>().TilePosition);
            var potentialTargets = EntitiesFor(PotentialTargetKey);
            var targetCenter = SingleEntityFor(ReticleKey).GetComponent<TileLocation>().TilePosition;
            var map = SingleEntityFor(MapKey).GetComponent<Map>();

            var targetingOutcomes = TargetUtils.GetTargetingOutcomes(map, ptState.SelectedSkill, SingleEntityFor(SelectedKey), potentialTargets, targetCenter, indicatorLocations);
            targetingOutcomes.ForEach(to => manager.AddComponentToEntity(to.Entity, to.Outcome));

            // TODO: This can probably be put into a separate system so it is handled automatically everywhere they are targeted.
            var firstTarget = targetingOutcomes.FirstOrDefault()?.Entity;
            manager.PerformHudAction("SetTargetingInfo", TargetUtils.BuildTargetingString(firstTarget?.GetComponent<Targeted>()));
            manager.PerformHudAction("SetProfile", Direction.Right, firstTarget);
            // TODO: Indicate if there are more than one
            // TODO: Deterministic sort by distance from center
            // TODO: Maybe here, maybe somewhere else, but display an on map indicator of which unit is displaying profile card
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
