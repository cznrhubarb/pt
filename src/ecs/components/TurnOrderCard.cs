using Ecs;
using Godot;

public class TurnOrderCard : Component
{
    private TurnOrderCardPrefab card;

    private TurnOrderCard() { }

    public int RemainingTicks
    {
        get => int.Parse(card.ValueLabel.Text);
        set => card.ValueLabel.Text = value.ToString();
    }

    public Vector2 Position
    {
        get => card.Position;
        set => card.Position = value;
    }

    // TODO: Don't make me add affiliation twice. We should pass in profile information here instead
    public static TurnOrderCard For(string portrait, Affiliation affiliation)
    {
        var prefab = ResourceLoader.Load<PackedScene>("res://prefabs/TurnOrderCardPrefab.tscn");
        var toc = new TurnOrderCard() { card = (TurnOrderCardPrefab)prefab.Instance() };
        toc.card.Init(portrait, affiliation);

        var hud = Globals.SceneTree.Root.FindNode("HUD", true, false);
        hud.AddChild(toc.card);

        return toc;
    }
}
