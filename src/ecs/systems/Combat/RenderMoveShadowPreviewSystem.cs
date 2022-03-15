using Ecs;
using Godot;
using System;
using System.Linq;

// Has to be after MarkTargetsSystem, ClampToMapSystem, and DepthSortSystem
public class RenderMoveShadowPreviewSystem : Ecs.System
{
    const string MapEntityKey = "map";

    public RenderMoveShadowPreviewSystem()
    {
        AddRequiredComponent<Targeted>();
        AddRequiredComponent<TileLocation>();

        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();

        var targetedComp = entity.GetComponent<Targeted>();
        if (targetedComp.TargetEffects.ContainsKey("Move"))
        {
            var newPos = (Vector3)targetedComp.TargetEffects["Move"];
            var currentColor = entity.Modulate / 2;
            entity.Modulate = Color.Color8((byte)currentColor.r8, (byte)currentColor.g8, (byte)currentColor.b8, 172);

            entity.Position = map.IsoMap.MapToWorld(newPos) + new Vector2(0, 24);
            var locationComp = entity.GetComponent<TileLocation>();
            entity.ZIndex = Convert.ToInt32((newPos.x + newPos.y + newPos.z) * DepthSortSystem.SpaceFactor) + locationComp.ZLayer;
        }
    }
}
