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
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<Map>(MapEntityKey);
        AddRequiredComponent<TravelLocation>(TravelLocationEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var target = entity.GetComponent<Reticle>().CurrentTarget;
        if (lastSelected != target)
        {
            ClearOldMarkers();

            if (target?.GetComponentOrNull<Movable>() != null)
            {
                var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();
                var moveStats = target.GetComponent<Movable>();
                var tileLocation = target.GetComponent<TileLocation>();

                var travelLocations = map.AStar.GetPointsInRange(moveStats, tileLocation.TilePosition);

                foreach (var spot in travelLocations)
                {
                    var ent = manager.GetNewEntity();
                    manager.AddComponentToEntity(ent, new TileLocation() { TilePosition = new Vector3(spot.x, spot.y, spot.z), ZLayer = 1 });
                    manager.AddComponentToEntity(ent, new SpriteWrap());
                    manager.AddComponentToEntity(ent, new TravelLocation());

                    ent.GetComponent<SpriteWrap>().Sprite.Texture = GD.Load("res://img/tiles/image_part_029.png") as Texture;
                }
            }

            lastSelected = target;
        }
        else if (target != null && target.GetComponentOrNull<Movable>()?.StartingLocation != null)
        {
            // Still the same target, but it moved on us.
            ClearOldMarkers();
        }
    }

    private void ClearOldMarkers()
    {
        foreach (var spot in EntitiesFor(TravelLocationEntityKey))
        {
            manager.DeleteEntity(spot.Id);
        }
    }
    }
