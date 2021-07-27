using Ecs;

public class ExplorationRoamState : State
{
    private Entity playerControlled;

    public ExplorationRoamState(Entity playerControlled)
    {
        this.playerControlled = playerControlled;
    }

    public override void Pre(Manager manager)
    {
        //manager.AddComponentsToEntity(playerControlled, new Selected());
    }

    public override void Post(Manager manager)
    {
        //manager.RemoveComponentFromEntity<Selected>(playerControlled);
    }
}
