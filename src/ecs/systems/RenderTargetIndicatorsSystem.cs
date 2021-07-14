using Ecs;
using Godot;
using System;
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
        AddRequiredComponent<TargetLocation>(ActionLocationEntityKey);
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
            var maxAoeHeightDelta = ptState.SelectedMove.MaxAoeHeightDelta;
            var points = map.AStar.GetPointsBetweenRange(reticleLocation.TilePosition, 0, areaRange);

            for (var i = 0; i < points.Count; i++)
            {
                if (Math.Abs(points[i].z - reticleLocation.TilePosition.z) <= maxAoeHeightDelta)
                {
                    // TODO: Because the actual sprite positioning doesn't happen until the next render loop,
                    //  this flashes at the wrong spot :(
                    //  Fixed by setting a "should be visible next frame" in the sprite wrap and letting the
                    //  other render system take care of it
                    indicators[i].GetComponent<TileLocation>().TilePosition = points[i];
                    indicators[i].GetComponent<SpriteWrap>().Sprite.Visible = true;
                }
            }
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }
}
