using Ecs;
using Godot;
using System.Collections.Generic;

public class ProfileCardPrefab : Control
{
    public Label HealthAmountLabel { get; private set; }

    private string lastCompleteAnimation;

    private Sprite portraitSprite;
    private Label nameLabel;
    private NinePatchRect backgroundRect;
    private AnimationPlayer animationPlayer;

    private Entity currentProfileEntity;

    public override void _Ready()
    {
        backgroundRect = FindNode("NinePatchRect") as NinePatchRect;
        portraitSprite = FindNode("Portrait") as Sprite;
        nameLabel = FindNode("Name") as Label;
        HealthAmountLabel = FindNode("HealthAmount") as Label;
        animationPlayer = FindNode("AnimationPlayer") as AnimationPlayer;
        animationPlayer.Connect("animation_finished", this, nameof(SetLastCompleteAnimation));
    }

    private void SetLastCompleteAnimation(string animationName)
    {
        lastCompleteAnimation = animationName;
    }

    public void MakeRoomForActionMenu()
    {
        if (animationPlayer.CurrentAnimation != "SlideUp" && lastCompleteAnimation != "SlideUp")
        {
            animationPlayer.Play("SlideUp");
        }
    }

    public void TakeAwayRoomForActionMenu()
    {
        if (animationPlayer.CurrentAnimation != "SlideDown" && lastCompleteAnimation != "SlideDown")
        {
            animationPlayer.Play("SlideDown");
        }
    }

    public void SetProfile(Entity profileEntity)
    {
        if (profileEntity == currentProfileEntity)
        {
            return;
        }

        if (profileEntity != null)
        {
            ValidateProfileEntity(profileEntity);

            if (currentProfileEntity == null || animationPlayer.IsPlaying())
            {
                currentProfileEntity = profileEntity;
                ShowNewProfile();
            }
            else
            {
                currentProfileEntity = profileEntity;
                animationPlayer.Play("SlideOut");
                animationPlayer.Connect("animation_finished", this, nameof(ShowNewProfile));
            }
        }
        else if (profileEntity == null)
        {
            animationPlayer.Play("SlideOut");
        }

        currentProfileEntity = profileEntity;
    }

    private void ValidateProfileEntity(Entity profileEntity)
    {
        // Required:
        //  Health: Health
        //  ProfileDetails: Name/portrait/affiliation
        //  FightStats: Other stats
        // Optional:
        //  Movable: Move/Jump
        //  MoveSet: Moves
        //  StatusBag: Status Effects
        //  Elemental: Element
        profileEntity.AssertComponentExists<ProfileDetails>();
        profileEntity.AssertComponentExists<Health>();
        profileEntity.AssertComponentExists<FightStats>();
    }

    private void ShowNewProfile(string _animationName = "")
    {
        var profileDetails = currentProfileEntity.GetComponent<ProfileDetails>();

        Color backgroundColor;
        switch (profileDetails.Affiliation)
        {
            case Affiliation.Enemy:
                backgroundColor = Color.Color8(221, 74, 63);
                break;
            case Affiliation.Friendly:
                backgroundColor = Color.Color8(107, 233, 138);
                break;
            case Affiliation.Neutral:
            default:
                backgroundColor = Color.Color8(120, 120, 120);
                break;
        }

        backgroundRect.Modulate = backgroundColor;
        portraitSprite.Texture = GD.Load<Texture>($"res://img/portraits/{profileDetails.MonNumber}.png");
        nameLabel.Text = profileDetails.Name;

        animationPlayer.Play("SlideIn");
        if (animationPlayer.IsConnected("animation_finished", this, nameof(ShowNewProfile)))
        {
            animationPlayer.Disconnect("animation_finished", this, nameof(ShowNewProfile));
        }

        var fightStats = currentProfileEntity.GetComponent<FightStats>();
        (GetNode("Strength") as Label).Text = $"STR {fightStats.Str}";
        (GetNode("Dexterity") as Label).Text = $"DEX {fightStats.Dex}";
        (GetNode("Toughness") as Label).Text = $"TUF {fightStats.Tuf}";
        (GetNode("Attunement") as Label).Text = $"ATN {fightStats.Atn}";
        (GetNode("Magic") as Label).Text = $"MAG {fightStats.Mag}";

        var health = currentProfileEntity.GetComponent<Health>();
        HealthAmountLabel.Text = $"{health.Current} / {health.Max}";
        (GetNode("HealthBar") as ProgressBar).Value = health.Current * 100f / health.Max;

        var movable = currentProfileEntity.GetComponentOrNull<Movable>();
        (GetNode("Movement") as Label).Text = $"MOV {movable?.MaxMove ?? 0}";
        (GetNode("Jump") as Label).Text = $"JMP {movable?.MaxJump ?? 0}";

        var statusList = currentProfileEntity.GetComponentOrNull<StatusBag>()?.StatusList ?? new List<StatusEffect>();

        var moves = currentProfileEntity.GetComponentOrNull<MoveSet>()?.Moves ?? new List<Move>();
        for (var i = 0; i < moves.Count; i++)
        {
            (GetNode($"MoveElement{i+1}") as Sprite).Texture = GD.Load<Texture>($"res://img/icons/element_{moves[i].Element.ToString().ToLower()}.png");
            (GetNode($"MoveName{i+1}") as Label).Text = moves[i].Name;
            (GetNode($"TpCount{i+1}") as Label).Text = $"{moves[i].CurrentTP} / {moves[i].MaxTP}";
        }

        var element = currentProfileEntity.GetComponentOrNull<Elemental>()?.Element ?? Element.Neutral;
        (GetNode("MonElement") as Sprite).Texture = GD.Load<Texture>($"res://img/icons/element_{element.ToString().ToLower()}.png");
    }
}
