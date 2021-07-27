using Ecs;
using System.Collections.Generic;

public class StatusBag : Component
{
    public List<StatusEffect> StatusList { get; set; } = new List<StatusEffect>();
}

public class StatusEffect
{
    public bool Positive { get; set; } = false;
    public int Count { get; set; } = 1;
    public string Name { get; set; }
    public bool Ticks { get; set; }
    // icon
    // effect
}
