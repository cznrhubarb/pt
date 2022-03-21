using System.Collections.Generic;

public class SkillLearnset
{
    public List<(int levelLearned, Skill skill)> LevelSkills { get; set; }
        = new List<(int levelLearned, Skill skill)>();

    public List<Skill> TutorSkills { get; set; } = new List<Skill>();
}
