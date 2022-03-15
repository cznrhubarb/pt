using Ecs;
using Godot;
using System.Linq;

public class MarkTargetsSystem : Ecs.System
{
    private const string PotentialTargetKey = "potentialTarget";
    private const string TargetedKey = "targeted";
    private const string SelectedKey = "selected";
    private const string ReticleKey = "reticle";

    public MarkTargetsSystem()
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
        // TODO: Might be good to do a dirty flag here, but need to make sure we don't introduce a bug that happens
        //  if the same list is used between turns or something

        var currentTargets = EntitiesFor(TargetedKey);
        foreach (var target in currentTargets)
        {
            manager.RemoveComponentFromEntity<Targeted>(target);
            target.Modulate = Colors.White;
        }

        if (manager.CurrentState is PlayerTargetingState ptState)
        {
            var indicatorLocations = EntitiesFor(PrimaryEntityKey)
                .Where(ind => ind.GetComponent<SpriteWrap>().Sprite.Visible)
                .Select(ind => ind.GetComponent<TileLocation>().TilePosition);
            var potentialTargets = EntitiesFor(PotentialTargetKey);
            var targetCenter = SingleEntityFor(ReticleKey).GetComponent<TileLocation>().TilePosition;

            TargetUtils.MarkTargetingOutcomes(manager, ptState.SelectedSkill, SingleEntityFor(SelectedKey), potentialTargets, targetCenter, indicatorLocations);
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
