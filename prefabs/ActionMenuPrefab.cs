using Godot;
using System;
using System.Collections.Generic;

public class ActionMenuPrefab : Control
{
    public Label TitleLabel { get; private set; }

    private Action<string> callback;

    public void Init(Action<string> buttonCallback)
    {
        callback = buttonCallback;
    }

    public override void _Ready()
    {
        TitleLabel = FindNode("Title") as Label;
    }

    private void OnButtonPressed(string actionName)
    {
        callback.Invoke(actionName);
    }

    public void SetButtons(List<string> actions)
    {
        var vbox = GetNode("Background/VBoxContainer") as Control;
        foreach (Node prevBtn in vbox.GetChildren())
        {
            prevBtn.QueueFree();
            vbox.RemoveChild(prevBtn);
        }

        foreach (var action in actions)
        {
            var btn = new Button();
            btn.Text = action;
            btn.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { action });
            vbox.AddChild(btn);
        }

        // Hard coding size per button only because the vbox rect size doesn't update immediately upon adding children
        var menuSize = new Vector2(vbox.RectSize.x + 40, actions.Count * 20 + (actions.Count - 1) * 4 + 40);
        (GetNode("Background") as Control).RectSize = menuSize;

        RectPosition = this.GetViewport().Size - menuSize - new Vector2(20, 40);
    }
}
