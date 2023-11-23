using Godot;
using System;

public class StatusEffect
{
    public string Name { get; set; } = "";
    public bool Positive { get; set; } = false;
    public bool Tickable { get; set; } = true;
    public bool Stacks { get; set; } = false;
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
        var effect = new StatusEffect { Name = name, Count = startingCount };
        switch (name)
        {
            case "Protect":
                effect.Icon = GD.Load("res://img/icons/status/elated.png") as Texture;
                effect.Positive = true;
                break;
            case "Shell":
                effect.Icon = GD.Load("res://img/icons/status/ghost.png") as Texture;
                effect.Positive = true;
                break;
            case "Haste":
                effect.Icon = GD.Load("res://img/icons/status/clone.png") as Texture;
                effect.Positive = true;
                break;
            case "Slow":
                effect.Icon = GD.Load("res://img/icons/status/numb.png") as Texture;
                break;
            case "Regen":
                effect.Icon = GD.Load("res://img/icons/status/bless.png") as Texture;
                effect.Positive = true;
                effect.Stacks = true;
                break;
            case "Poison":
                effect.Icon = GD.Load("res://img/icons/status/poison.png") as Texture;
                effect.Stacks = true;
                break;
            case "Blind":
                effect.Icon = GD.Load("res://img/icons/status/curse.png") as Texture;
                break;
            case "Silence":
                effect.Icon = GD.Load("res://img/icons/status/silence.png") as Texture;
                break;
            case "Sleep":
                effect.Icon = GD.Load("res://img/icons/status/sleep.png") as Texture;
                break;
            case "Immobilize":
                effect.Icon = GD.Load("res://img/icons/status/stone.png") as Texture;
                break;
            case "Captured":
                effect.Icon = GD.Load("res://img/icons/status/gloom.png") as Texture;
                effect.Tickable = false;
                break;
            case "Uncaptureable":
                effect.Icon = GD.Load("res://img/icons/status/resolve.png") as Texture;
                effect.Positive = true;
                effect.Tickable = false;
                break;
            case "Capturing":
                effect.Icon = GD.Load("res://img/icons/status/charm.png") as Texture;
                effect.Positive = true;
                effect.Tickable = false;
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
    * 
    * 
    *       Not tickable
    * Captured
    * Capturing
    * Uncaptureable
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