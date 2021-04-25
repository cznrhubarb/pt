using Ecs;
using Godot;

public class ProfileCard : Component
{
    private ProfileCardPrefab card;

    public string HealthDisplay
    {
        get => card.HealthAmountLabel.Text;
        set => card.HealthAmountLabel.Text = value;
    }

    private ProfileCard() { }

    public static ProfileCard For(string name, string portrait, Affiliation affiliation)
    {
        var prefab = ResourceLoader.Load<PackedScene>("res://prefabs/ProfileCardPrefab.tscn");
        var pc = new ProfileCard() { card = (ProfileCardPrefab)prefab.Instance() };
        pc.card.Init(name, portrait, affiliation);

        var hud = Globals.SceneTree.Root.FindNode("HUD", true, false);
        hud.AddChild(pc.card);

        return pc;
    }
}
