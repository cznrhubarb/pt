using Ecs;
using Godot;

public class ApplyPositionOffsetSystem : Ecs.System
{
    // TODO: This must run after the ClampToMapSystem. Perhaps we should add some dependency type of thing to systems
    //  in this case to make sure I don't actually put them out of alignment later?
    public ApplyPositionOffsetSystem()
    {
        AddRequiredComponent<Offset>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        entity.Position += entity.GetComponent<Offset>().Amount;
    }
}
