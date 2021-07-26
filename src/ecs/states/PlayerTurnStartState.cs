using Ecs;
using Godot;
using System.Linq;

public class PlayerTurnStartState : State
{
    private Entity acting;
    private Entity map;

    public PlayerTurnStartState(Entity acting, Entity map)
    {
        this.acting = acting;
        this.map = map;
    }

    public override void Pre(Manager manager)
    {
        StatusEffectUpkeep();

        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () => manager.ApplyState(new PlayerMovementState(acting, map))
        });
    }

    public override void Post(Manager manager)
    {
    }

    private void StatusEffectUpkeep()
    {
        if (acting.HasComponent<StatusBag>())
        {
            var statusBag = acting.GetComponent<StatusBag>();
            statusBag.StatusList = statusBag.StatusList.Select(status =>
            {
                if (status.Ticks)
                {
                    status.Count--;
                }
                return status;
            })
                .Where(status => !status.Ticks || status.Count > 0)
                .ToList();
        }
    }
}
