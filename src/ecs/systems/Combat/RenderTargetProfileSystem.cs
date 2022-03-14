using Ecs;
using System.Linq;

public class RenderTargetProfileSystem : Ecs.System
{
    private const string PotentialTargetKey = "potentialTarget";
    private const string TargetedKey = "targeted";
    private const string SelectedKey = "selected";
    private const string ReticleKey = "reticle";

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
            var targetCenter = EntitiesFor(ReticleKey).First().GetComponent<TileLocation>().TilePosition;
            TargetUtils.MarkTargets(manager, ptState.SelectedSkill, SingleEntityFor(SelectedKey), potentialTargets, targetCenter, indicatorLocations);
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
