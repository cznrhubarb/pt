using Ecs;
using Godot;

public class ApplyDirectionToSpriteSystem : Ecs.System
{
    public ApplyDirectionToSpriteSystem()
    {
        AddRequiredComponent<Directionality>();
        AddRequiredComponent<SpriteWrap>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var direction = (int)entity.GetComponent<Directionality>().Direction;
        entity.GetComponent<SpriteWrap>().Sprite.Frame = direction;
    }
}
