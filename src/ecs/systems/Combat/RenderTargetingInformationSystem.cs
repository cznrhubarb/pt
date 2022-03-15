using Ecs;
using Godot;
using System;
using System.Linq;

// Has to be after MarkTargetsSystem
public class RenderTargetingInformationSystem : Ecs.System
{
    private const string SelectedKey = "selected";

    private int displayIndex = 0;

    public RenderTargetingInformationSystem()
    {
        AddRequiredComponent<Targeted>();

        AddRequiredComponent<Selected>(SelectedKey);
        AddRequiredComponent<Targeted>(SelectedKey);
    }

    public override void UpdateAll(float deltaTime)
    {
        // TODO: Might be good to do a dirty flag here, but need to make sure we don't introduce a bug that happens
        //  if the same list is used between turns or something

        var actingEntity = EntitiesFor(SelectedKey).FirstOrDefault();

        var otherTargets = EntitiesFor(PrimaryEntityKey)
            .Where(ent => ent != actingEntity);

        if (Input.IsActionJustPressed("profile_left"))
        {
            displayIndex--;
        }
        else if (Input.IsActionJustPressed("profile_right"))
        {
            displayIndex++;
        }

        var otherCount = otherTargets.Count();
        if (otherCount > 0)
        {
            if (displayIndex >= otherCount)
            {
                displayIndex %= otherCount;
            }
            else if (displayIndex < 0)
            {
                displayIndex += otherCount;
            }
        }

        var displayTarget = otherTargets.ElementAtOrDefault(displayIndex);

        manager.PerformHudAction("SetTargetingInfo", Direction.Left, BuildTargetingString(actingEntity?.GetComponent<Targeted>()));
        manager.PerformHudAction("SetTargetingInfo", Direction.Right, BuildTargetingString(displayTarget?.GetComponent<Targeted>()));
        manager.PerformHudAction("SetProfile", Direction.Right, displayTarget);

        foreach (var target in otherTargets)
        {
            // TODO: Need a better way to display this, probably animated
            var currentAlpha = (byte)target.Modulate.a8;
            var color = Colors.Orange;
            if (target == displayTarget)
            {
                color = Colors.MediumVioletRed;
            }
            target.Modulate = color;
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }

    private static string BuildTargetingString(Targeted targeted)
    {
        if (targeted == null)
        {
            return "";
        }

        var effectsToDisplay = targeted.TargetEffects.Where(fx => fx.Key != "Move");
        if (effectsToDisplay.Count() == 0)
        {
            return "";
        }

        string targetingString = $"{Math.Min(100, targeted.HitChance)}%";
        foreach (var kvp in effectsToDisplay)
        {
            if (kvp.Value != null)
            {
                targetingString += $"   {kvp.Value} {kvp.Key}";
            }
            else
            {
                targetingString += $"   {kvp.Key}";
            }
        }

        return targetingString;
    }
}
