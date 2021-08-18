using Ecs;
using System.Collections.Generic;

public class StatusBag : Component
{
    public Dictionary<string, StatusEffect> Statuses { get; set; } = new Dictionary<string, StatusEffect>();
}
