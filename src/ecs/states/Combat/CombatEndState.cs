using Ecs;
using Godot;
using System.Linq;

public class CombatEndState : State
{
    public CombatEndState(EndCondition endCondition)
    {
    }

    public override void Pre(Manager manager)
    {
    }

    public override void Post(Manager manager)
    {
    }

    // No leaving this without a scene change
    public override bool CanTransitionTo<T>()
    {
        return false;
    }
}

public enum EndCondition
{
    Win,
    Lose
}
