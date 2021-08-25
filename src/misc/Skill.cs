﻿using Godot;
using MonoCustomResourceRegistry;
using System;
using System.Collections.Generic;

// TODO: Maybe need a new icon here
[RegisteredType(nameof(Skill), "res://editoricons/Component.svg", nameof(Resource))]
public class Skill : Resource
{
    [Export]
    public string Name { get; set; }
    [Export]
    public Element Element { get; set; }
    public int CurrentTP { get; set; }
    [Export]
    public int MaxTP { get; set; }
    [Export]
    public int MinRange { get; set; }
    [Export]
    public int MaxRange { get; set; }
    [Export]
    public int MaxHeightRangeUp { get; set; } = 2;
    [Export]
    public int MaxHeightRangeDown { get; set; } = 2;
    [Export]
    public int AreaOfEffect { get; set; } = 0;
    [Export]
    public int MaxAoeHeightDelta { get; set; } = 0;
    // Lower is better
    [Export]
    public int Speed { get; set; } = 0;
    [Export]
    public bool Physical { get; set; } = true;

    [Export]
    public int Accuracy { get; set; }
    [Export]
    public int CritModifier { get; set; } = 0;

    [Export]
    public Dictionary<string, int> Effects { get; set; }

    ///Special bonus things
    //targeting type (line, etc) if we want something other than the radiate out shape pattern
    //affiliation restriction, if we want to make something only impact one type or another
}
