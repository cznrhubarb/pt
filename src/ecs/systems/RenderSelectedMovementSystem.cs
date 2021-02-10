using Ecs;
using Godot;
using System;

public class RenderSelectedMovementSystem : Ecs.System
{
    private const string MapEntityKey = "map";
    private const string TravelLocationEntityKey = "travelLocation";

    private Entity lastSelected = null;

    public RenderSelectedMovementSystem()
    {
        AddRequiredComponent<Selected>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<MoveStats>();
        AddRequiredComponent<Map>(MapEntityKey);
        AddRequiredComponent<TravelLocation>(TravelLocationEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        if (lastSelected != entity)
        {
            if (lastSelected != null)
            {
                foreach (var spot in EntitiesFor(TravelLocationEntityKey))
                {
                    manager.DeleteEntity(spot.Id);
                }
            }

            if (entity != null)
            {
                var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();
                var moveStats = entity.GetComponent<MoveStats>();
                var tileLocation = entity.GetComponent<TileLocation>();

                var startingPoint = map.AStar.GetClosestPoint(
                    new Vector3(tileLocation.TilePosition.x, tileLocation.TilePosition.y, tileLocation.Height));
                var travelLocations = map.AStar.GetPointsInRange(moveStats, startingPoint);

                foreach (var spot in travelLocations)
                {
                    var ent = manager.GetNewEntity();
                    manager.AddComponentToEntity(ent, new TileLocation() { Height = Convert.ToInt32(spot.z), TilePosition = new Vector2(spot.x, spot.y), ZLayer = 1 });
                    manager.AddComponentToEntity(ent, new SpriteWrap());
                    manager.AddComponentToEntity(ent, new TravelLocation());

                    ent.GetComponent<SpriteWrap>().Sprite.Texture = GD.Load("res://img/tiles/image_part_029.png") as Texture;
                }
            }

            lastSelected = entity;
        }
    }
}
