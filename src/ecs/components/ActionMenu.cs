using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class ActionMenu : Component
{
    private ActionMenuPrefab menu;

    public List<string> Actions { set => menu.SetButtons(value); }

    public string SelectedMenuAction { get; set; } = null;
    public bool Visible { get => menu.Visible; set => menu.Visible = value; }

    public ActionMenu()
    {
        var prefab = ResourceLoader.Load<PackedScene>("res://prefabs/ActionMenuPrefab.tscn");
        menu = prefab.Instance() as ActionMenuPrefab;
        menu.Init(selected => { SelectedMenuAction = selected; });

        var hud = Globals.SceneTree.Root.FindNode("HUD", true, false);
        hud.AddChild(menu);
    }
}
