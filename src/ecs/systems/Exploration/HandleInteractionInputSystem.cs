using Ecs;
using Godot;

public class HandleInteractionInputSystem : Ecs.System
{
    private const string InteractableKey = "interactable";

    public HandleInteractionInputSystem()
    {
        AddRequiredComponent<Selected>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Directionality>();
        AddRequiredComponent<InteractTrigger>(InteractableKey);
        AddRequiredComponent<TileLocation>(InteractableKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        if (entity.HasComponent<Tweening>())
        {
            return;
        }

        if (Input.IsActionJustPressed("ui_select"))
        {
            var testLocation = entity.GetComponent<TileLocation>().TilePosition
                + entity.GetComponent<Directionality>().Direction.ToVector3();
            var interactables = EntitiesFor(InteractableKey);

            foreach (var target in interactables)
            {
                if (WithinInteractionRange(testLocation, target.GetComponent<TileLocation>().TilePosition))
                {
                    var triggerComp = target.GetComponent<InteractTrigger>();
                    // TODO/HACK: Strong coupling. Fixed by moving TriggerCue out to Manager
                    //  but that comes with the extra cost of having to move all the dialog
                    //  stuff currently in exploration only to combat also :(
                    if (manager is Exploration ex)
                    {
                        ex.TriggerCue(triggerComp.Cue, triggerComp.CueParam);
                    }
                    break;
                }
            }
        }
    }

    // Assumes maximum z variance of 1 (instead of jump range) for interacting
    private bool WithinInteractionRange(Vector3 testPos, Vector3 targetPos) => 
        testPos.x == targetPos.x && testPos.y == targetPos.y && Mathf.Abs(testPos.z - targetPos.z) <= 1;
}
