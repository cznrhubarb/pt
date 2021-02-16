using Godot;

public class TurnOrderCardPrefab : Node2D
{
    public Label ValueLabel { get; private set; }

    private string portraitPath;
    private string cardPath;

    public void Init(string portrait, Affiliation affiliation)
    {
        portraitPath = $"res://img/portraits/{portrait}.png";
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
        (FindNode("Portrait") as Sprite).Texture = GD.Load<Texture>(portraitPath);
        ValueLabel = FindNode("ValueLabel") as Label;
    }
}
