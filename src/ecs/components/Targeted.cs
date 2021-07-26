using Ecs;
using System.Collections.Generic;

public class Targeted : Component
{
    public float HitChance { get; set; }
    public float CritChance { get; set; } = 0;
    public Dictionary<string, object> Effects { get; set; } = new Dictionary<string, object>();
}
