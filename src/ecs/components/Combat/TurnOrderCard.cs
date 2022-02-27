using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(TurnOrderCard), "res://editoricons/Component.svg", nameof(Resource))]
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

    public static TurnOrderCard For(ProfileDetails profileDetails, Affiliation affiliation)
    {
        var prefab = ResourceLoader.Load<PackedScene>("res://prefabs/TurnOrderCardPrefab.tscn");
        var toc = new TurnOrderCard() { card = (TurnOrderCardPrefab)prefab.Instance() };
        toc.card.Init(profileDetails.ProfilePicture, affiliation);

        var hud = Globals.SceneTree.Root.FindNode("HUD", true, false);
        hud.AddChild(toc.card);

        return toc;
    }

    public void Cleanup()
    {
        card.QueueFree();
    }
}
