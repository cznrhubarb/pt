using Ecs;
using Godot;
using System;

public class MonInfo : Control
{
    private int gainShown;
    private Label experienceLabel;
    private Label experienceGainLabel;
    private ProgressBar experienceBar;
    private MonsterState monsterStateRef;

    private int levelsGained = 0;

    public bool FinishedAnimating { get; private set; } = true;

    public override void _Ready()
    {
        GetLabel("LevelUpLabel").Visible = false;
        GetLabel("HpGainLabel").Visible = false;
        GetLabel("StrGainLabel").Visible = false;
        GetLabel("MagGainLabel").Visible = false;
        GetLabel("DexGainLabel").Visible = false;
        GetLabel("TufGainLabel").Visible = false;
        GetLabel("AtnGainLabel").Visible = false;
        GetLabel("LevelGainLabel").Visible = false;
        GetNode<Button>("NewSkillButton").Visible = false;
        GetNode<Button>("NewEvoButton").Visible = false;
        experienceLabel = GetLabel("ExperienceLabel");
        experienceGainLabel = GetLabel("ExperienceGainLabel");
        experienceGainLabel.Visible = false;
        experienceBar = GetNode<ProgressBar>("ExperienceBar");
    }

    public void SetBaseValues(MonsterState monState)
    {
        monsterStateRef = monState;

        GetNode<Sprite>("Sprite").Texture = monState.Blueprint.Sprite;
        GetLabel("NameLabel").Text = monState.CustomName;
        GetLabel("LevelLabel").Text = $"Lv. {monState.Level}";
        GetLabel("HpLabel").Text = $"HP\n{monState.MaxHealth}";
        GetLabel("StrLabel").Text = $"STR\n{monState.Str}";
        GetLabel("MagLabel").Text = $"MAG\n{monState.Mag}";
        GetLabel("DexLabel").Text = $"DEX\n{monState.Dex}";
        GetLabel("TufLabel").Text = $"TUF\n{monState.Tuf}";
        GetLabel("AtnLabel").Text = $"ATN\n{monState.Atn}";
        experienceLabel.Text = $"{monState.Experience} XP";
        experienceBar.Value = monState.Experience;
    }

    public void SetExperienceGain(int gainedExperience)
    {
        FinishedAnimating = false;
        gainShown = 0;

        experienceGainLabel.Text = "";
        experienceGainLabel.Visible = true;
        var timer = new System.Timers.Timer() { Interval = 50, AutoReset = true, Enabled = true };
        timer.Elapsed += (s, e) => {
            gainShown++;
            monsterStateRef.Experience++;
            experienceGainLabel.Text = $"+ {gainShown}";

            if (monsterStateRef.Experience == 100)
            {
                // TODO: Determine if there are new evos available
                // TODO: Determine if there are new skills the mon has learned
                // TODO?: Make those evo and skill buttons do something. (or make them labels?)
                //      Maybe they just show the things and don't allow you to change things in this menu
                SetLevelUpValues();
                monsterStateRef.Experience = 0;
            }

            experienceLabel.Text = $"{monsterStateRef.Experience} XP";
            experienceBar.Value = monsterStateRef.Experience;

            if (gainShown >= gainedExperience)
            {
                // TODO: Maybe shouldn't have this tied to the animation finishing, but it'll do for now
                monsterStateRef.Level += levelsGained;
                monsterStateRef.RecalculateStats();

                FinishedAnimating = true;
                timer.Stop();
            }
        };
        timer.Start();
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

        levelsGained++;
        var statGrowth = monsterStateRef.DetermineStatGainFromLevelUp(levelsGained);

        GetLabel("LevelUpLabel").Visible = true;
        GetLabel("LevelGainLabel").Visible = true;
        GetLabel("LevelGainLabel").Text = $"+{levelsGained}";
        GetLabel("HpGainLabel").Text = $"+{statGrowth.Health}";
        GetLabel("StrGainLabel").Text = $"+{statGrowth.Str}";
        GetLabel("MagGainLabel").Text = $"+{statGrowth.Mag}";
        GetLabel("DexGainLabel").Text = $"+{statGrowth.Dex}";
        GetLabel("TufGainLabel").Text = $"+{statGrowth.Tuf}";
        GetLabel("AtnGainLabel").Text = $"+{statGrowth.Atn}";
        GetLabel("HpGainLabel").Visible = true;
        GetLabel("StrGainLabel").Visible = true;
        GetLabel("MagGainLabel").Visible = true;
        GetLabel("DexGainLabel").Visible = true;
        GetLabel("TufGainLabel").Visible = true;
        GetLabel("AtnGainLabel").Visible = true;
    }

    private Label GetLabel(string nodeName) => GetNode(nodeName) as Label;
}
