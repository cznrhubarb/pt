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
                    var spotEnt = manager.GetNewEntity();
                    manager.AddComponentToEntity(spotEnt, new TileLocation() { TilePosition = new Vector3(spot.x, spot.y, spot.z), ZLayer = 1 });
                    manager.AddComponentToEntity(spotEnt, new SpriteWrap());
                    manager.AddComponentToEntity(spotEnt, new TravelLocation());
                    manager.AddComponentToEntity(spotEnt, new Pulse() { squishAmountX = 0.05f, squishAmountY = 0.05f, squishSpeed = 2.5f });

                    Texture tex = null;
                    if (target.HasComponent<PlayerCharacter>())
                    {
                        tex = GD.Load("res://img/tiles/image_part_029.png") as Texture;
                    }
                    else if (target.HasComponent<FriendlyNpc>())
                    {
                        tex = GD.Load("res://img/tiles/image_part_031.png") as Texture;
                    }
                    else if (target.HasComponent<EnemyNpc>())
                    {
                        tex = GD.Load("res://img/tiles/image_part_030.png") as Texture;
                    }
                    var sprite = spotEnt.GetComponent<SpriteWrap>().Sprite;
                    sprite.Modulate = new Color(1, 1, 1, 0.65f);
                    sprite.Texture = tex;
                }
            }

            lastSelected = target;
        }
        else if (target?.GetComponentOrNull<Movable>()?.StartingLocation != null)
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
