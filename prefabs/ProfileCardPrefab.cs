using Ecs;
using Godot;

public class ProfileCardPrefab : Control
{
    public Label HealthAmountLabel { get; private set; }

    private Color cachedBackgroundColor;
    private string cachedPortraitPath;
    private string cachedName;
    private string lastCompleteAnimation;

    private Sprite portraitSprite;
    private Label nameLabel;
    private NinePatchRect backgroundRect;
    private AnimationPlayer animationPlayer;

    private object currentProfile;

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
        if (profileEntity == currentProfile)
        {
            return;
        }

        if (profileEntity != null)
        {
            // if we're setting this to a new profile, and we're in the middle of animating out OR in,
            // just start from the beginning of SlideIn. It'll be jittery, but that will be visually understandable
            // from a user point of view. (I'm pretty sure.)

            ValidateProfileEntity(profileEntity);
            CacheProfileValues(profileEntity);

            if (currentProfile == null || animationPlayer.IsPlaying())
            {
                ShowNewProfile();
            }
            else
            {
                animationPlayer.Play("SlideOut");
                animationPlayer.Connect("animation_finished", this, nameof(ShowNewProfile));
            }
        }
        else if (profileEntity == null)
        {
            animationPlayer.Play("SlideOut");
        }

        currentProfile = profileEntity;
    }

    private void ShowNewProfile(string _animationName = "")
    {
        backgroundRect.Modulate = cachedBackgroundColor;
        portraitSprite.Texture = GD.Load<Texture>(cachedPortraitPath);
        nameLabel.Text = cachedName;

        animationPlayer.Play("SlideIn");
        if (animationPlayer.IsConnected("animation_finished", this, nameof(ShowNewProfile)))
        {
            animationPlayer.Disconnect("animation_finished", this, nameof(ShowNewProfile));
        }
    }

    private void ValidateProfileEntity(Entity profileEntity)
    {
        profileEntity.AssertComponentExists<ProfileDetails>();
        profileEntity.AssertComponentExists<Health>();
    }

    private void CacheProfileValues(Entity profileEntity)
    {
        var profileDetails = profileEntity.GetComponent<ProfileDetails>();

        cachedName = profileDetails.Name;
        cachedPortraitPath = $"res://img/portraits/{profileDetails.MonNumber}.png";
        switch (profileDetails.Affiliation)
        {
            case Affiliation.Enemy:
                cachedBackgroundColor = Color.Color8(221, 74, 63);
                break;
            case Affiliation.Friendly:
                cachedBackgroundColor = Color.Color8(107, 233, 138);
                break;
            case Affiliation.Neutral:
            default:
                cachedBackgroundColor = Color.Color8(120, 120, 120);
                break;
        }
    }
}
