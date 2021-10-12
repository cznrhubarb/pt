using Godot;

public class TurnOrderCardPrefab : Node2D
{
    public Label ValueLabel { get; private set; }

    private Texture portrait;
    private string cardPath;

    public void Init(Texture portrait, Affiliation affiliation)
    {
        this.portrait = portrait;
        switch (affiliation)
        {
            case Affiliation.Enemy:
                cardPath = "res://img/ui/metalPanel_redCorner.png";
                break;
            case Affiliation.Friendly:
                cardPath = "res://img/ui/metalPanel_blueCorner.png";
                break;
            case Affiliation.Neutral:
                cardPath = "res://img/ui/metalPanel_yellowCorner.png";
                break;
        }
    }

    public override void _Ready()
    {
        (FindNode("CardBack") as TextureRect).Texture = GD.Load<Texture>(cardPath);
        (FindNode("Portrait") as Sprite).Texture = portrait;
        ValueLabel = FindNode("ValueLabel") as Label;
    }
}
