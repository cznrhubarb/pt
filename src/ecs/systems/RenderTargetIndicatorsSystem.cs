using Ecs;
using Godot;
using System.Linq;

public class RenderTargetIndicatorsSystem : Ecs.System
{
    private const string ActionLocationEntityKey = "actionLocation";
    private const string ReticleEntityKey = "reticle";
    private const string MapEntityKey = "map";

    public RenderTargetIndicatorsSystem()
    {
        AddRequiredComponent<TargetIndicator>();
        AddRequiredComponent<SpriteWrap>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<TravelLocation>(ActionLocationEntityKey);
        AddRequiredComponent<TileLocation>(ActionLocationEntityKey);
        AddRequiredComponent<Reticle>(ReticleEntityKey);
        AddRequiredComponent<Map>(MapEntityKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        var indicators = EntitiesFor(PrimaryEntityKey);
        var reticleLocation = SingleEntityFor(ReticleEntityKey).GetComponent<TileLocation>();
        var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();

        indicators.ForEach(i => i.GetComponent<SpriteWrap>().Sprite.Visible = false);

        var potentialLocations = EntitiesFor(ActionLocationEntityKey)
            .Where(ent => ent.Visible)
            .Select(ent => ent.GetComponent<TileLocation>());

        var targetLocation = potentialLocations.FirstOrDefault(
            location =>
             location.TilePosition == reticleLocation.TilePosition);

        if (targetLocation != null && manager.CurrentState is PlayerTargetingState ptState)
        {
            // HACK: Strong coupling here.
            var areaRange = ptState.SelectedMove.AreaOfEffect;
            var points = map.AStar.GetPointsBetweenRange(reticleLocation.TilePosition, 0, areaRange);

            for (var i = 0; i < points.Count; i++)
            {
                indicators[i].GetComponent<SpriteWrap>().Sprite.Visible = true;
                indicators[i].GetComponent<TileLocation>().TilePosition = points[i];
            }
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
