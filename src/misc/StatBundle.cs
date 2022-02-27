
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(StatBundle), "res://editoricons/Component.svg", nameof(Resource))]
public class StatBundle : Resource
{
    [Export]
    public int Health { get; set; } = 0;
    [Export]
    public int Atn { get; set; } = 0;
    [Export]
    public int Dex { get; set; } = 0;
    [Export]
    public int Mag { get; set; } = 0;
    [Export]
    public int Str { get; set; } = 0;
    [Export]
    public int Tuf { get; set; } = 0;

    public static StatBundle operator +(StatBundle left, StatBundle right)
    {
        return new StatBundle()
        {
            Health = left.Health + right.Health,
            Atn = left.Atn + right.Atn,
            Dex = left.Dex + right.Dex,
            Mag = left.Mag + right.Mag,
            Str = left.Str + right.Str,
            Tuf = left.Tuf + right.Tuf,
        };
    }

    public static StatBundle operator -(StatBundle left, StatBundle right)
    {
        return new StatBundle()
        {
            Health = left.Health - right.Health,
            Atn = left.Atn - right.Atn,
            Dex = left.Dex - right.Dex,
            Mag = left.Mag - right.Mag,
            Str = left.Str - right.Str,
            Tuf = left.Tuf - right.Tuf,
        };
    }

    public static StatBundle operator *(StatBundle left, int right)
    {
        return new StatBundle()
        {
            Health = left.Health * right,
            Atn = left.Atn * right,
            Dex = left.Dex * right,
            Mag = left.Mag * right,
            Str = left.Str * right,
            Tuf = left.Tuf * right,
        };
    }

    public static StatBundle operator /(StatBundle left, int right)
    {
        return new StatBundle()
        {
            Health = left.Health / right,
            Atn = left.Atn / right,
            Dex = left.Dex / right,
            Mag = left.Mag / right,
            Str = left.Str / right,
            Tuf = left.Tuf / right,
        };
    }
}