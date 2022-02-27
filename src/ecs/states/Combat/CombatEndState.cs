using Ecs;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CombatEndState : State
{
    private EndCondition endCondition;

    public CombatEndState(EndCondition endCondition)
    {
        this.endCondition = endCondition;
    }

    public override void Pre(Manager manager)
    {
        Node screen;
        switch (endCondition)
        {
            case EndCondition.Win:
                {
                    var spoils = manager.GetEntitiesWithComponent<CombatSpoils>().First().GetComponent<CombatSpoils>();
                    var captured = manager.GetEntitiesWithComponent<Captured>()
                                          .FirstOrDefault()?.GetComponentOrNull<ProfileDetails>()?.MonsterState;
                    screen = ResourceLoader.Load<PackedScene>("res://prefabs/WinScreen.tscn").Instance();
                    (screen as WinScreen).Init(spoils.DeathPool[Affiliation.Friendly], spoils.DeathPool[Affiliation.Enemy], spoils.FoundItems, captured);
                }
                break;
            case EndCondition.Lose:
                {
                    screen = ResourceLoader.Load<PackedScene>("res://prefabs/LoseScreen.tscn").Instance();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException("Unsupported EndCondition given: " + endCondition);
        }

        var hud = Globals.SceneTree.Root.FindNode("HUD", true, false);
        hud.AddChild(screen);
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
