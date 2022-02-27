using Ecs;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class WinScreen : CanvasLayer
{
    private List<MonInfo> monInfos = new List<MonInfo>();

    private List<Item> foundItems;

    private List<MonsterState> friendlyDeaths;
    private List<MonsterState> defeatedMonsters;
    private MonsterState capturedMonster;

    public override void _Ready()
    {
        // Setup Monster info
        monInfos.Add(GetNode("MonInfo1") as MonInfo);
        monInfos.Add(GetNode("MonInfo2") as MonInfo);
        monInfos.Add(GetNode("MonInfo3") as MonInfo);
        monInfos.Add(GetNode("MonInfo4") as MonInfo);

        var partnershipGrowth = WorldState.PartyState.Aggregate(
            new StatBundle(), (bundle, state) => state.Blueprint.PartnershipGrowth + bundle);

        for (var i = 0; i < 4; i++)
        {
            if (i < WorldState.PartyState.Count)
            {
                var partyMember = WorldState.PartyState[i];
                monInfos[i].SetBaseValues(partyMember);
                if (!friendlyDeaths.Contains(partyMember))
                {
                    // Living members get partnership growth from all members (dead or alive)
                    partyMember.Partnership += partnershipGrowth;
                    monInfos[i].SetExperienceGain(DetermineExperienceGain(partyMember));
                }
            }
            else
            {
                monInfos[i].Visible = false;
            }
        }

        // Setup Item info
        if (foundItems.Count > 0)
        {
            (GetNode("ItemsWindow/NoItemsLabel") as Label).Visible = false;
            var gridContainer = GetNode("ItemsWindow/GridContainer") as GridContainer;
            for (var i = 0; i < foundItems.Count; i++)
            {
                var itemTex = new TextureRect() { Texture = foundItems[i].Sprite };
                gridContainer.AddChild(itemTex);
            }

            WorldState.Inventory.AddRange(foundItems);
        }

        // Setup captured monster info
        if (capturedMonster != null)
        {
            (GetNode("CaptureWindow/NameLabel") as Label).Text = capturedMonster.Blueprint.Name;
            (GetNode("CaptureWindow/LevelLabel") as Label).Text = $"LV {capturedMonster.Level}";
            (GetNode("CaptureWindow/HpLabel") as Label).Text = $"HP\n{capturedMonster.MaxHealth}";
            (GetNode("CaptureWindow/StrLabel") as Label).Text = $"STR\n{capturedMonster.Str}";
            (GetNode("CaptureWindow/DexLabel") as Label).Text = $"DEX\n{capturedMonster.Dex}";
            (GetNode("CaptureWindow/AtnLabel") as Label).Text = $"ATN\n{capturedMonster.Atn}";
            (GetNode("CaptureWindow/MagLabel") as Label).Text = $"MAG\n{capturedMonster.Mag}";
            (GetNode("CaptureWindow/TufLabel") as Label).Text = $"TUF\n{capturedMonster.Tuf}";
            (GetNode("CaptureWindow/ProfilePic") as TextureRect).Texture = capturedMonster.Blueprint.ProfilePicture;
            WorldState.MonsterBank.Add(capturedMonster);
        }
        else
        {
            (GetNode("CaptureWindow") as Control).Visible = false;
        }
    }

    private int DetermineExperienceGain(MonsterState partyMember) =>
        defeatedMonsters.Sum(kill => Mathf.Max(1, Mathf.Clamp((kill.Level - partyMember.Level) * 5 + 25, 1, 100) / WorldState.PartyState.Count));

    public void Init(List<MonsterState> deaths, List<MonsterState> defeated, List<Item> items, MonsterState captured)
    {
        friendlyDeaths = deaths;
        defeatedMonsters = defeated;
        foundItems = items;
        capturedMonster = captured;
    }
}
