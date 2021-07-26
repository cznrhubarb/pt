using Godot;
using System;
using System.Collections.Generic;

public class ActionMenuPrefab : Control
{
    private List<Button> moveButtons = new List<Button>();
    private Action<Move> callback;

    public override void _Ready()
    {
        moveButtons.Add(GetNode("MoveButton1") as Button);
        moveButtons.Add(GetNode("MoveButton2") as Button);
        moveButtons.Add(GetNode("MoveButton3") as Button);
        moveButtons.Add(GetNode("MoveButton4") as Button);
        GetNode("WaitButton").Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { null });
    }

    public void SetButtonCallback(Action<Move> buttonCallback)
    {
        callback = buttonCallback;
    }

    private void OnButtonPressed(Move action)
    {
        callback.Invoke(action);
    }

    public void RegisterMoveSet(MoveSet moveSet)
    {
        for (var i = 0; i < moveButtons.Count; i++)
        {
            moveButtons[i].Visible = moveSet?.Moves.Count > i;
            if (moveButtons[i].Visible)
            {
                moveButtons[i].Disabled = moveSet.Moves[i].CurrentTP == 0;
                if (moveButtons[i].IsConnected("pressed", this, nameof(OnButtonPressed)))
                {
                    moveButtons[i].Disconnect("pressed", this, nameof(OnButtonPressed));
                }
                moveButtons[i].Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { moveSet?.Moves[i] });
            }
        }
    }
}
