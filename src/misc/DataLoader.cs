using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DataLoader
{
    public static readonly Dictionary<int, MonsterBlueprint> BlueprintData = new Dictionary<int, MonsterBlueprint>();
    public static readonly Dictionary<int, Skill> SkillData = new Dictionary<int, Skill>();
    public static readonly Dictionary<string, CutScene> CutSceneData = new Dictionary<string, CutScene>();

    public static void LoadAll()
    {
        // Creatures
        // Skills
        // Evolution lines
        // Skill learnsets
        // Move learnsets
        // Cutscenes

        // Creatures must be loaded before evolution lines and learnsets
        // Skills must be loaded before skill learnsets

        // TODO: Currently have a GScript to csv, zip, and download this all with two clicks,
        //  but it would be nice if it were single click and also moved the files to this data folder.
        //  Probably need to make it code in here instead of in GSheets to get there.
        LoadFile("res://data/creatures.csv", ParseMonsterBlueprint);
        LoadFile("res://data/skills.csv", ParseSkill);
        LoadFile("res://data/evolution_lines.csv", ParseEvolutionPath);
        LoadFile("res://data/skill_learnsets.csv", ParseSkillLearnset);
        LoadFile("res://data/movable_learnsets.csv", ParseMovableLearnset);
        LoadFile("res://data/cut_scenes.csv", ParseCutScene);
    }

    private static void LoadFile(string fileName, Action<string[]> parseMethod, int maxCount = -1)
    {
        GD.Print("Loading " + fileName);
        var count = 0;
        var file = new File();
        file.Open(fileName, File.ModeFlags.Read);
        // Ignore the first line (csv header info)
        file.GetLine();
        while (!file.EofReached())
        {
            parseMethod(file.GetCsvLine());
            if (++count == maxCount) break;
        }
        file.Close();
    }

    // Maybe there's a way to template the Parse method, if I have a way of correlating csv data with properties?
    //  A generic type they all derive off of perhaps that keeps a map?
    private static void ParseMonsterBlueprint(string[] csvData)
    {
        //ID	Name	Type	Element	Rarity	baseHp	baseStr	baseMag	baseTuf	baseDex	baseAtn	partnerHp	partnerStr	partnerMag	partnerTuf	partnerDex	partnerAtn	FunFact
        var blueprint = new MonsterBlueprint()
        {
            MonNumber = int.Parse(csvData[0]),
            Name = csvData[1],
            MonsterType = csvData[2].ToEnum<MonsterType>(),
            Element = csvData[3].ToEnum<Element>(),
            Rarity = csvData[4].ToEnum<RarityValue>(),
            Base = new StatBundle()
            {
                Health = int.Parse(csvData[5]),
                Str = int.Parse(csvData[6]),
                Mag = int.Parse(csvData[7]),
                Tuf = int.Parse(csvData[8]),
                Dex = int.Parse(csvData[9]),
                Atn = int.Parse(csvData[10]),
            },
            PartnershipGrowth = new StatBundle()
            {
                Health = int.Parse(csvData[11]),
                Str = int.Parse(csvData[12]),
                Mag = int.Parse(csvData[13]),
                Tuf = int.Parse(csvData[14]),
                Dex = int.Parse(csvData[15]),
                Atn = int.Parse(csvData[16]),
            },
            Description = csvData[17]
        };

        blueprint.ProfilePicture = ResourceLoader.Load<Texture>($"res://img/portraits/{blueprint.MonNumber}.png");
        blueprint.Sprite = ResourceLoader.Load<Texture>($"res://img/sprites/{blueprint.MonNumber}.png");

        BlueprintData.Add(blueprint.MonNumber, blueprint);
    }

    private static void ParseSkill(string[] csvData)
    {
        //ID Name    Element MaxTp   TargetingMode MinRange    MaxRange MaxHeightUp MaxHeightDown MinAoeRange MaxAoeRange IgnoreUser  MaxAoeHeightDelta Speed   IsPhysical Accuracy    CritModifier TargetEffects   UserEffects Description
        var skill = new Skill()
        {
            Id = int.Parse(csvData[0]),
            Name = csvData[1],
            Element = csvData[2].ToEnum<Element>(),
            MaxTP = int.Parse(csvData[3]),
            TargetingMode = csvData[4].ToEnum<TargetingMode>(),
            MinRange = int.Parse(csvData[5]),
            MaxRange = int.Parse(csvData[6]),
            MaxHeightRangeUp = int.Parse(csvData[7]),
            MaxHeightRangeDown = int.Parse(csvData[8]),
            MinAoeRange = int.Parse(csvData[9]),
            MaxAoeRange = int.Parse(csvData[10]),
            IgnoreUser = bool.Parse(csvData[11]),
            MaxAoeHeightDelta = int.Parse(csvData[12]),
            Speed = int.Parse(csvData[13]),
            Physical = bool.Parse(csvData[14]),
            Accuracy = int.Parse(csvData[15]),
            CritModifier = int.Parse(csvData[16]),
            TargetEffects = SplitKeyValuePairString(csvData[17]).ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2),
            SelfEffects = SplitKeyValuePairString(csvData[18]).ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2),
            Description = csvData[19]
        };
        SkillData.Add(skill.Id, skill);
    }

    private static void ParseEvolutionPath(string[] csvData)
    {
        // Leaving evo line ID in for now, even though we don't use it.
        // var evoLineId = int.Parse(csvData[0]);

        //EvoLineID	FromMonsterID	ToMonsterID	EvolutionMethod	Params
        var fromMonsterId = int.Parse(csvData[1]);
        var evoPath = new EvolutionPath(int.Parse(csvData[2]), csvData[3].ToEnum<EvolutionMethod>(), csvData[4]);
        BlueprintData[fromMonsterId].EvolutionPaths.Add(evoPath);
    }

    private static void ParseSkillLearnset(string[] csvData)
    {
        //MonsterID	SkillID	LevelLearned	LearnMethod
        var blueprint = BlueprintData[int.Parse(csvData[0])];
        var skill = SkillData[int.Parse(csvData[1])];
        var learnMethod = csvData[3].ToEnum<SkillLearnMethod>();
        switch (learnMethod)
        {
            case SkillLearnMethod.LevelUp:
                blueprint.SkillLearnset.LevelSkills.Add((int.Parse(csvData[2]), skill));
                break;
            case SkillLearnMethod.Tutor:
                blueprint.SkillLearnset.TutorSkills.Add(skill);
                break;
        }
    }

    private static void ParseMovableLearnset(string[] csvData)
    {
        //MonsterID LevelLearned    MaxMove MaxJump TravelSpeed TerrainCostModifiers
        var blueprint = BlueprintData[int.Parse(csvData[0])];
        var levelLearned = int.Parse(csvData[1]);
        var movable = new Movable()
        {
            MaxMove = int.Parse(csvData[2]),
            MaxJump = int.Parse(csvData[3]),
            TravelSpeed = int.Parse(csvData[4]),
            TerrainCostModifiers = SplitKeyValuePairString(csvData[5]).ToDictionary(kvp => kvp.Item1.ToEnum<TerrainType>(), kvp => float.Parse(kvp.Item2))
        };

        blueprint.MoveStatsByLevel.Add(levelLearned, movable);
    }

    private static void ParseCutScene(string[] csvData)
    {
        //Scene Name	Scene Step	Event	Params
        var scene = CutSceneData.GetOrCreate(csvData[0], new CutScene());
        var sceneStep = int.Parse(csvData[1]);
        CutSceneEvent cutSceneEvent = null;

        // TODO: Should actor paths be replaced with something a little easier like node name?
        switch (csvData[2])
        {
            case "ChangeScene":
                cutSceneEvent = new CSEChangeScene() { SceneName = csvData[3] };
                break;
            case "MoveActorAbsolute":
                cutSceneEvent = new CSEMoveActorAbsolute() { ActorPath = csvData[3], FinalPosition = ToVector3(csvData[4]) };
                break;
            case "MoveActorToTarget":
                cutSceneEvent = new CSEMoveActorToTarget() { ActorPath = csvData[3], TargetPath = csvData[4] };
                break;
            case "RemoveActor":
                cutSceneEvent = new CSERemoveActor() { ActorPath = csvData[3] };
                break;
            case "StartDialog":
                cutSceneEvent = new CSEStartDialog() { DialogName = csvData[3] };
                break;
            case "TurnActorAbsolute":
                cutSceneEvent = new CSETurnActorAbsolute() { ActorPath = csvData[3], Direction = csvData[4].ToEnum<Direction>() };
                break;
            case "TurnActorToTarget":
                cutSceneEvent = new CSETurnActorToTarget() { ActorPath = csvData[3], TargetPath = csvData[4] };
                break;
        }

        if (scene.Events.Count < sceneStep)
        {
            scene.Events.Resize(sceneStep);
        }
        // Offset by one because we have starting index of 1 in sheet
        scene.Events[sceneStep - 1] = cutSceneEvent;
    }

    private static Vector3 ToVector3(string vecString)
    {
        var pieces = vecString.Split(',').Select(s => float.Parse(s.Trim()));
        return new Vector3(pieces.ElementAt(0), pieces.ElementAt(1), pieces.ElementAt(2));
    }

    private static List<(string, string)> SplitKeyValuePairString(string kvpString)
    {
        return kvpString
            .Split(',')
            .Where(kvp => !string.IsNullOrEmpty(kvp))
            .Select(kvp =>
            {
                var pcs = kvp.Split(':').Select(s => s.Trim());
                return (pcs.ElementAt(0), pcs.ElementAtOrDefault(1));
            })
            .ToList();
    }
}
