using Godot;
using System;

public class ProfileCardPrefab : Control
{
    public Label HealthAmountLabel { get; private set; }

    private string name;
    private string portraitPath;
    private Color backgroundColor;

    public void Init(string name, string portrait, Affiliation affiliation)
    {
        this.name = name;
        portraitPath = $"res://img/portraits/{portrait}.png";
        switch (affiliation)
        {
            case Affiliation.Enemy:
                backgroundColor = Color.Color8(221, 74, 63);
                break;
            case Affiliation.Friendly:
                backgroundColor = Color.Color8(107, 233, 138);
                break;
            case Affiliation.Neutral:
                backgroundColor = Color.Color8(120, 120, 120);
                break;
        }
    }

    public override void _Ready()
    {
        (FindNode("NinePatchRect") as NinePatchRect).Modulate = backgroundColor;
        (FindNode("Portrait") as Sprite).Texture = GD.Load<Texture>(portraitPath);
        (FindNode("Name") as Label).Text = name;
        HealthAmountLabel = FindNode("HealthAmount") as Label;
    }
}
