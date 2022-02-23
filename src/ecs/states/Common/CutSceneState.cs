using Ecs;
using Godot;
using System.Linq;

public class CutSceneState : State
{
    CutScene cutScene;
    int eventIndex = 0;
    bool terminatesWithSceneChange = false;
    bool finished = false;

    public CutSceneState(string cutSceneName)
    {
        cutScene = GD.Load<CutScene>($"res://res/cutscenes/{cutSceneName}.tres");
    }

    public override void Pre(Manager manager)
    {
        RunNextStep(manager);
    }

    public override void Post(Manager manager)
    {
    }

    public override bool CanTransitionTo<T>()
    {
        return finished;
    }

    private void RunNextStep(Manager manager)
    {
        if (eventIndex < cutScene.Events.Length)
        {
            var nextEvent = cutScene.Events[eventIndex++];
            if (nextEvent is CSEChangeScene)
            {
                terminatesWithSceneChange = true;
            }
            nextEvent.Manager = manager;
            nextEvent.OnComplete = () => RunNextStep(manager);
            GD.Print(nextEvent.GetType());
            nextEvent.RunStep();
        }
        else
        {
            finished = true;
            if (!terminatesWithSceneChange)
            {
                manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
                {
                    Callback = () => manager.ApplyState(new ExplorationRoamState(manager.GetEntitiesWithComponent<Selected>().First())),
                    Delay = 0f
                });
            }
        }
    }
}
