using Godot;
using System;
using System.Collections.Generic;

public class ActionMenuPrefab : Control
{
    private List<Button> skillButtons = new List<Button>();
    private Action<Skill> callback;

    public override void _Ready()
    {
        skillButtons.Add(GetNode("SkillButton1") as Button);
        skillButtons.Add(GetNode("SkillButton2") as Button);
        skillButtons.Add(GetNode("SkillButton3") as Button);
        skillButtons.Add(GetNode("SkillButton4") as Button);
        GetNode("WaitButton").Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { null });
    }

    public void SetButtonCallback(Action<Skill> buttonCallback)
    {
        callback = buttonCallback;
    }

    private void OnButtonPressed(Skill action)
    {
        callback.Invoke(action);
    }

    public void RegisterSkillSet(SkillSet skillSet, bool silenced)
    {
        for (var i = 0; i < skillButtons.Count; i++)
        {
            skillButtons[i].Visible = skillSet?.Skills.Count > i;
            if (skillButtons[i].Visible)
            {
                skillButtons[i].Disabled = skillSet.Skills[i].CurrentTP == 0 || (!skillSet.Skills[i].Physical && silenced);
                if (skillButtons[i].IsConnected("pressed", this, nameof(OnButtonPressed)))
                {
                    skillButtons[i].Disconnect("pressed", this, nameof(OnButtonPressed));
                }
                skillButtons[i].Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { skillSet?.Skills[i] });
            }
        }
    }
}
