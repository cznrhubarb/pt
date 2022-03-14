using Ecs;
using Godot;
using System.Collections.Generic;

public class ProfileCardPrefab : Control
{
    public Label HealthAmountLabel { get; private set; }
    public string TargetingInfo { set => targetingInfo.Text = value; }

    private string lastCompleteAnimation;

    private Sprite portraitSprite;
    private Label targetingInfo;
    private Label nameLabel;
    private NinePatchRect backgroundRect;
    private AnimationPlayer animationPlayer;
    private Sprite[] skillElementSprites;
    private Label[] skillNameLabels;
    private Label[] skillTpLabels;

    private Entity currentProfileEntity;
    public bool MatchesCurrentEntity(Entity match) => match == currentProfileEntity;

    public override void _Ready()
    {
        backgroundRect = FindNode("NinePatchRect") as NinePatchRect;
        portraitSprite = FindNode("Portrait") as Sprite;
        nameLabel = FindNode("Name") as Label;
        HealthAmountLabel = FindNode("HealthAmount") as Label;
        animationPlayer = FindNode("AnimationPlayer") as AnimationPlayer;
        targetingInfo = FindNode("TargetingInfo") as Label;
        animationPlayer.Connect("animation_finished", this, nameof(SetLastCompleteAnimation));

        skillElementSprites = new Sprite[]
        {
            GetNode("SkillElement1") as Sprite,
            GetNode("SkillElement2") as Sprite,
            GetNode("SkillElement3") as Sprite,
            GetNode("SkillElement4") as Sprite
        };
        skillNameLabels = new Label[] 
        {
            GetNode("SkillName1") as Label,
            GetNode("SkillName2") as Label,
            GetNode("SkillName3") as Label,
            GetNode("SkillName4") as Label
        };
        skillTpLabels = new Label[]
        {
            GetNode("TpCount1") as Label,
            GetNode("TpCount2") as Label,
            GetNode("TpCount3") as Label,
            GetNode("TpCount4") as Label
        };
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
                ShowNewProfile("SlideOut");
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
            TargetingInfo = "";
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
        //  SkillSet: Skills
        //  StatusBag: Status Effects
        //  Elemental: Element
        profileEntity.AssertComponentExists<ProfileDetails>();
        profileEntity.AssertComponentExists<Health>();
        profileEntity.AssertComponentExists<FightStats>();
    }

    private void ShowNewProfile(string animationName = "")
    {
        // Just in case we finish an animation but cleared this out
        if (currentProfileEntity == null || animationName != "SlideOut")
        {
            return;
        }

        var profileDetails = currentProfileEntity.GetComponent<ProfileDetails>();
        var affiliation = currentProfileEntity.GetComponent<Affiliated>().Affiliation;

        Color backgroundColor;
        switch (affiliation)
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
        portraitSprite.Texture = profileDetails.ProfilePicture;
        nameLabel.Text = profileDetails.Name;
        (GetNode("Level") as Label).Text = $"Lvl {profileDetails.Level}";

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

        // TODO: Should show status effect be in here, and the application of it call update?
        var silenced = currentProfileEntity.GetComponent<StatusBag>().Statuses.ContainsKey("Silence");

        var skills = currentProfileEntity.GetComponentOrNull<SkillSet>()?.Skills ?? new List<Skill>();
        for (var i = 0; i < skills.Count; i++)
        {
            skillElementSprites[i].Texture = GD.Load<Texture>($"res://img/icons/element_{skills[i].Element.ToString().ToLower()}.png");
            skillNameLabels[i].Text = skills[i].Name;
            skillTpLabels[i].Text = $"{skills[i].CurrentTP} / {skills[i].MaxTP}";

            skillElementSprites[i].Visible = true;
            skillNameLabels[i].Visible = true;
            skillTpLabels[i].Visible = true;

            var modColor = new Color(1, 1, 1);
            if (skills[i].CurrentTP == 0 || (!skills[i].Physical && silenced))
            {
                modColor = new Color(0.5f, 0.5f, 0.5f);
            }
            skillElementSprites[i].Modulate = modColor;
            skillNameLabels[i].Modulate = modColor;
            skillTpLabels[i].Modulate = modColor;
        }
        for (var i = skills.Count; i < 4; i++)
        {
            skillElementSprites[i].Visible = false;
            skillNameLabels[i].Visible = false;
            skillTpLabels[i].Visible = false;
        }

        var element = currentProfileEntity.GetComponentOrNull<Elemental>()?.Element ?? Element.Neutral;
        (GetNode("MonElement") as Sprite).Texture = GD.Load<Texture>($"res://img/icons/element_{element.ToString().ToLower()}.png");
    }

    public void SetStatusEffects(StatusBag statusBag)
    {
        var positiveStatuses = GetNode("PositiveStatuses") as HBoxContainer;
        var negativeStatuses = GetNode("NegativeStatuses") as HBoxContainer;

        foreach (var sNode in positiveStatuses.GetChildren()) { (sNode as Node).QueueFree(); }
        foreach (var sNode in negativeStatuses.GetChildren()) { (sNode as Node).QueueFree(); }

        foreach (var status in statusBag.Statuses)
        {
            var sNode = new TextureRect();
            sNode.Expand = true;
            sNode.RectSize = new Vector2(24, 24);
            sNode.RectMinSize = new Vector2(24, 24);
            sNode.Texture = status.Value.Icon;

            if (status.Value.Positive)
            {
                positiveStatuses.AddChild(sNode);
            }
            else
            {
                negativeStatuses.AddChild(sNode);
            }
        }
    }

    public void FlashMove(int moveNumber)
    {
        var count = 10;
        var timer = new System.Timers.Timer(150);
        timer.Elapsed += (o,e) =>
        {
            skillElementSprites[moveNumber].Visible = !skillElementSprites[moveNumber].Visible;
            skillNameLabels[moveNumber].Visible = !skillNameLabels[moveNumber].Visible;
            skillTpLabels[moveNumber].Visible = !skillTpLabels[moveNumber].Visible;
            count--;
            if (count <= 0) { timer.Enabled = false; }
        };
        timer.AutoReset = true;
        timer.Enabled = true;
    }
}
