﻿using Godot;
using System.Collections.Generic;

public static class Constants
{
    public static readonly CaptureValues Capture = new CaptureValues();

    public class CaptureValues
    {
        public readonly float HealthFloor = 0.1f;
        public readonly float HealthCeiling = 0.8f;
        public readonly float HealthFloorValue = 0.5f;
        public readonly float HealthCeilingValue = 0.0f;
        public readonly float TrainerLevelMod = 0.0025f;
        public readonly Dictionary<RarityValue, float> RarityMods = new Dictionary<RarityValue, float>()
        {
            { RarityValue.Common, 0.6f },
            { RarityValue.Uncommon, 0.2f },
            { RarityValue.Rare, -0.2f },
            { RarityValue.Epic, -0.35f },
            { RarityValue.Legendary, -0.65f },
        };
        public readonly Dictionary<string, float> StatusMods = new Dictionary<string, float>()
        {
            { "Sleep", 0.25f },
            { "Immobilize", 0.15f },
            { "Slow", 0.05f },
            { "Haste", -0.05f },
        };
    }
}