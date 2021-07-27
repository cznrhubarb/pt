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
}

public enum EndCondition
{
    Win,
    Lose
}
