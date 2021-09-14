using Godot;
using System;

public class StatusEffect
{
    public string Name { get; set; } = "";
    public bool Positive { get; set; } = false;
    public bool Tickable { get; set; } = true;
    public Texture Icon { get; set; } = null;
    public int Count { get; set; } = 1;
    // effect??

    public const float ProtectDamageModifier = 0.65f;
    public const float ShellDamageModifier = 0.65f;
    public const float PoisonDamagePortion = 0.125f;
    public const float RegenHealPortion = 0.1f;
    public const float BlindAccuracyModifier = 0.4f;
}

// TODO: Change these strings to enums
public class StatusFactory
{
    public static StatusEffect BuildStatusEffect(string name, int startingCount)
    {
        var effect = new StatusEffect();
        effect.Name = name;
        effect.Count = startingCount;
        switch (name)
        {
            case "Protect":
                effect.Icon = GD.Load("res://img/icon/status/elated.png") as Texture;
                break;
            case "Shell":
                effect.Icon = GD.Load("res://img/icon/status/ghost.png") as Texture;
                break;
            case "Haste":
                effect.Icon = GD.Load("res://img/icon/status/clone.png") as Texture;
                break;
            case "Slow":
                effect.Icon = GD.Load("res://img/icon/status/numb.png") as Texture;
                break;
            case "Regen":
                effect.Icon = GD.Load("res://img/icon/status/bless.png") as Texture;
                break;
            case "Poison":
                effect.Icon = GD.Load("res://img/icon/status/poison.png") as Texture;
                break;
            case "Blind":
                effect.Icon = GD.Load("res://img/icon/status/curse.png") as Texture;
                break;
            case "Silence":
                effect.Icon = GD.Load("res://img/icon/status/silence.png") as Texture;
                break;
            case "Sleep":
                effect.Icon = GD.Load("res://img/icon/status/sleep.png") as Texture;
                break;
            case "Immobilize":
                effect.Icon = GD.Load("res://img/icon/status/stone.png") as Texture;
                break;
            default:
                throw new ArgumentException($"Invalid status effect being built: {name}");
        }

        return effect;
    }
}

// WHAT WE WILL ADD FOR NOW
/*
    *      All of these are tickable
    * Protect
    * Shell
    * Haste
    * Slow
    * Regen
    * Sleep
    * Poison
    * Blind
    * Silence
    * Immobilize
    */

// OTHER GAMES
/*
    * Charging
    * Performing
    * Defending
    * Jumping
    * 
    * Float
    * Reraise
    * Invisible
    * Regen
    * Protect
    * Shell
    * Haste
    * 
    * Sleep
    * Poison
    * Blind
    * Oil
    * Stone
    * Confuse
    * Silence
    * Toad
    * Slow
    * Stop
    * Immobilize
    * Disable
    * Doom
    * Vampire
    * Undead
    * Chicken
    * Charm
    * 
    * Berserk
    * Athiest
    * Faith
    * Reflect
    */

/*
    * Paralyzed
    * Poison
    * Badly Poisoned
    * Burned
    * Frozen
    * Flinch
    * Confused
    * Infatuation
    * Leech Seed
    */

/*
    * Energy Shield
    * Fire Immunity
    * Flying
    * Kickoff Boosters
    * Natural Armor
    * Smoke Immunity
    * Zoltan Shield
    * 
    * ACID
    * Fire
    * Frozen
    * Not Powered
    * Webbed
    * 
    * Smoke
    * Submerged
    */