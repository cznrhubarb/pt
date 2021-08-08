using Ecs;
using Godot;

public class TacticalPlan
{
    public Vector3 MoveTargetLocation { get; set; }
    public Skill SelectedSkill { get; set; }
    public Vector3 SkillTargetLocation { get; set; }
}

public class Tactician
{
    public static TacticalPlan GetTurnPlan(Entity acting)
    {
        // MOCK
        return new TacticalPlan()
        {
            MoveTargetLocation = new Vector3(5, 0, 2),
            SelectedSkill = acting.GetComponentOrNull<SkillSet>().Skills[0],
            SkillTargetLocation = new Vector3(5, 1, 2)
        };
    }
}
