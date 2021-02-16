using Ecs;
using Godot;

public class TurnOrderCard : Component
{
    public TurnOrderCardPrefab Card { get; private set; }

    private TurnOrderCard() { }

    public int RemainingTicks
    {
        get => int.Parse(Card.ValueLabel.Text);
        set => Card.ValueLabel.Text = value.ToString();
    }

    public static TurnOrderCard For(string portrait, Affiliation affiliation)
    {
        var prefab = (PackedScene)ResourceLoader.Load("res://prefabs/TurnOrderCardPrefab.tscn");
        var toc = new TurnOrderCard() { Card = (TurnOrderCardPrefab)prefab.Instance() };
        // TODO: Maybe look at NOTIFICATION_INSTANCED signal to auto add to hud?
        toc.Card.Init(portrait, affiliation);
        return toc;
    }
}
