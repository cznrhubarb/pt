using Ecs;
using Godot;
using System;
using System.Linq;

public class StatusTickEventSystem : Ecs.System
{
    public StatusTickEventSystem()
    {
        AddRequiredComponent<StatusTickEvent>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var bag = entity.GetComponent<StatusTickEvent>().TickingEntity.GetComponentOrNull<StatusBag>();

        if (bag != null)
        {
            bag.Statuses = bag.Statuses.Select(status =>
            {
                if (status.Value.Tickable)
                {
                    status.Value.Count--;
                }

                switch (status.Value.Name)
                {
                    case "Regen":
                        {
                            var healthComp = entity.GetComponent<Health>();
                            var healAmount = (int)(healthComp.Max * StatusEffect.RegenHealPortion);
                            healthComp.Current = Math.Max(healthComp.Max, healthComp.Current + healAmount);
                            FactoryUtils.BuildTextEffect(manager, entity.GetComponent<TileLocation>().TilePosition, healAmount.ToString(), new Color(0.5f, 0.9f, 0.3f));
                        }
                        break;
                    case "Poison":
                        {
                            var healthComp = entity.GetComponent<Health>();
                            var damageAmount = (int)(healthComp.Max * StatusEffect.PoisonDamagePortion);
                            healthComp.Current -= Math.Min(healthComp.Current, damageAmount);
                            FactoryUtils.BuildTextEffect(manager, entity.GetComponent<TileLocation>().TilePosition, damageAmount.ToString(), new Color(0.9f, 0.2f, 0.8f));
                            if (healthComp.Current == 0)
                            {
                                manager.AddComponentToEntity(entity, new Dying());
                            }
                        }
                        break;
                    default:
                        break;
                }


                return status;
            })
                .Where(status => status.Value.Count > 0)
                .ToDictionary(status => status.Key, status => status.Value);
        }

        // TODO: Make an EventSystem subclass of System that handles cleanup automatically
        manager.DeleteEntity(entity.Id);
    }
}
