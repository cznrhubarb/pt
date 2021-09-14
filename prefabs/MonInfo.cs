using Ecs;
using Godot;
using System;

public class MonInfo : Control
{
    public override void _Ready()
    {
        GetLabel("LevelUpLabel").Visible = false;
        GetLabel("HpGainLabel").Visible = false;
        GetLabel("StrGainLabel").Visible = false;
        GetLabel("MagGainLabel").Visible = false;
        GetLabel("DexGainLabel").Visible = false;
        GetLabel("TufGainLabel").Visible = false;
        GetLabel("AtnGainLabel").Visible = false;
        GetLabel("ExperienceGainLabel").Visible = false;
        GetNode<Button>("NewSkillButton").Visible = false;
        GetNode<Button>("NewEvoButton").Visible = false;
    }

    public void SetBaseValues(MonsterState monState)
    {
        GetNode<TextureRect>("Sprite").Texture = monState.Blueprint.Sprite;
        GetLabel("NameLabel").Text = monState.CustomName;
        GetLabel("LevelLabel").Text = $"Lv. {monState.Level}";
        GetLabel("HpLabel").Text = $"HP\n{monState.MaxHealth}";
        GetLabel("StrLabel").Text = $"STR\n{monState.Str}";
        GetLabel("MagLabel").Text = $"MAG\n{monState.Mag}";
        GetLabel("DexLabel").Text = $"DEX\n{monState.Dex}";
        GetLabel("TufLabel").Text = $"TUF\n{monState.Tuf}";
        GetLabel("AtnLabel").Text = $"ATN\n{monState.Atn}";
        GetLabel("ExperienceLabel").Text = "00 XP";
        GetNode<ProgressBar>("ExperienceBar").Value = 0;
    }

    public void SetLevelUpValues()
    {
        // TODO: Animate the level up.
        //  Progress bar ticks up
        //  XP gain label ticks down as XP label ticks up (with progress)
        //  Level up label appears when it hits 100%
        //  Level up label punches out (on each level up)
        //  Gain labels also appear and punch each level up
        //  Gain labels keep cumulative total gains until the leveling is all done
        //  After all xp is gained, all gain labels "merge" into true stat labels
        //  Duplicate all gain labels with shadow versions so that players can still see the deltas
        //  Shadow labels for stat and level increases aren't seen until the numbers merge
        //  Shadow label for xp gain is placed under the xp gain label as it ticks down
        //  New Evo/Skill buttons punch in and become visible if appropriate

        GetLabel("LevelUpLabel").Visible = true;
        GetLabel("HpGainLabel").Text = $"+{1}";
        GetLabel("StrGainLabel").Text = $"+{1}";
        GetLabel("MagGainLabel").Text = $"+{1}";
        GetLabel("DexGainLabel").Text = $"+{1}";
        GetLabel("TufGainLabel").Text = $"+{1}";
        GetLabel("AtnGainLabel").Text = $"+{1}";
        GetLabel("HpGainLabel").Visible = true;
        GetLabel("StrGainLabel").Visible = true;
        GetLabel("MagGainLabel").Visible = true;
        GetLabel("DexGainLabel").Visible = true;
        GetLabel("TufGainLabel").Visible = true;
        GetLabel("AtnGainLabel").Visible = true;
        GetLabel("ExperienceGainLabel").Text = $"+{1}";
        GetLabel("ExperienceGainLabel").Visible = true;
        GetNode<ProgressBar>("ExperienceBar").Value = 0;
    }

    private Label GetLabel(string nodeName) => GetNode(nodeName) as Label;
}
