using Godot;
using Ecs;
using System.Collections.Generic;
using System;

public class Exploration : Manager
{
    private const float DialogDelay = 0.5f;

    private float dialogDelayTimer = 0f;

    public override void _Ready()
    {
        ApplyState(new ExplorationRoamState(null));
        CreateSystems();
        BuildControlElements();
        var mapComp = BuildMap();
        BuildActors();
        MapUtils.RefreshObstacles(mapComp, GetEntitiesWithComponent<TileLocation>());

        AnchorCamera();

        foreach (var entity in GetEntitiesWithComponent<AutorunTrigger>())
        {
            // TODO: Should this technically be in a system?
            var triggerComp = entity.GetComponent<AutorunTrigger>();
            TriggerCue(triggerComp.Cue, triggerComp.CueParam);
        }
    }

    private void CreateSystems()
    {
        AddSystem(new FollowCameraControlSystem());
        AddSystem(new ClampToMapSystem());
        AddSystem(new ApplyPositionOffsetSystem());
        AddSystem(new ApplyDirectionToSpriteSystem());
        AddSystem(new DepthSortSystem());
        AddSystem(new TweenCleanupSystem());
        AddSystem(new CameraAnchoringSystem());

        // Event Handling Systems
        AddSystem(new CreateShockwaveEventSystem());
        AddSystem(new DeferredEventSystem());

        AddSystem<ExplorationRoamState>(new HandleMovementInputSystem());
        AddSystem<ExplorationRoamState>(new HandleInteractionInputSystem());
    }

    private Map BuildMap()
    {
        var mapNode = FindNode("Map");
        var tileToTerrain = new Dictionary<int, TerrainType>()
        {
            { 0, TerrainType.Dirt },
            { 1, TerrainType.Dirt },
            { 2, TerrainType.Dirt },
            { 3, TerrainType.Grass },
            { 4, TerrainType.Grass },
            { 5, TerrainType.Grass },
            { 6, TerrainType.Grass },
            { 7, TerrainType.Stone },
            { 8, TerrainType.Stone },
            { 9, TerrainType.Stone },
            { 10, TerrainType.Stone },
            { 11, TerrainType.Water },
            { 12, TerrainType.Water },
            { 13, TerrainType.DeepWater },
            { 14, TerrainType.Sand },
            { 15, TerrainType.Sand },
            { 16, TerrainType.Wood },
            { 17, TerrainType.Wood },
            { 18, TerrainType.Wood },
            { 19, TerrainType.Wood },
        };

        // TODO: Should this be moved to IsoMap? We are duplicating the code here and in Combat
        var tileEntities = new List<Entity>();
        var height = 0;
        foreach (var child in mapNode.GetChildren())
        {
            if (child is TileMap map)
            {
                var tileSet = map.TileSet;
                foreach (Vector2 tile in map.GetUsedCells())
                {
                    var tileId = map.GetCellv(tile);

                    var tileEnt = GetNewEntity();
                    AddComponentsToEntity(tileEnt, 
                        new TileLocation() { TilePosition = new Vector3(tile.x, tile.y, height), ZLayer = 0 }, 
                        new SpriteWrap(), 
                        new Terrain() { Type = tileToTerrain[tileId] });
                    tileEntities.Add(tileEnt);

                    tileEnt.GetComponent<SpriteWrap>().Sprite.Texture = map.TileSet.TileGetTexture(tileId);
                }

                height++;
            }
        }

        var mapEnt = GetNewEntity();
        var mapComp = new Map(tileEntities);
        AddComponentToEntity(mapEnt, mapComp);

        mapNode.QueueFree();

        return mapComp;
    }

    private void BuildControlElements()
    {
        RegisterExistingEntity(FindNode("Camera") as Entity);
    }

    private void BuildActors()
    {
        foreach (var entity in FindNode("Actors").GetChildren())
        {
            RegisterExistingEntity(entity as Entity);
        }
    }

    public override void PerformHudAction(string actionName, params object[] args)
    {
        switch (actionName)
        {
            default:
                throw new ArgumentException($"Attempting to perform an illegal HUD action: {actionName}");
        }
    }

    public void TriggerCue(CueType cueType, string cueParameter)
    {
        switch (cueType)
        {
            case CueType.StartDialog:
                StartDialog(cueParameter);
                break;
            case CueType.ChangeScene:
                Transition.To($"res://src/scenes/{cueParameter}.tscn");
                break;
            case CueType.StartCutScene:
                ApplyState(new CutSceneState(cueParameter));
                break;
            default:
                throw new ArgumentException($"Attempting to trigger unknown cue: {cueType}");
        }
    }

    private void StartDialog(string dialogName)
    {
        if (CurrentState is ExplorationRoamState && dialogDelayTimer <= 0)
        {
            ApplyState(new DialogState());
            var dialog = DialogicSharp.Start(dialogName, false);
            AddChild(dialog);
            dialog.Connect("dialogic_signal", this, nameof(DialogSignal));
            dialog.Connect("timeline_end", this, nameof(DialogFinished));
        }
    }

    private void DialogSignal(string parameter)
    {
        // TODO: Obviously this is all hardcoded and needs to be made generic
        switch (parameter)
        {
            case "ChooseBulb":
                WorldState.PartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Bulbasaur.tres"), 1));
                WorldState.RivalPartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Charmander.tres"), 1));
                ApplyState(new CutSceneState("RivalChoosesChar"));
                break;
            case "ChooseSquirt":
                WorldState.PartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Squirtle.tres"), 1));
                WorldState.RivalPartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Bulbasaur.tres"), 1));
                ApplyState(new CutSceneState("RivalChoosesBulb"));
                break;
            case "ChooseChar":
                WorldState.PartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Charmander.tres"), 1));
                WorldState.RivalPartyState.Add(MonsterFactory.BuildMonster(GD.Load<MonsterBlueprint>("res://res/monsters/Squirtle.tres"), 1));
                ApplyState(new CutSceneState("RivalChoosesSquirt"));
                break;
            case "StartFight":
                Transition.To("res://src/scenes/Combat.tscn");
                break;
        }
    }

    private void DialogFinished(string _timelineName)
    {
        ApplyState(new ExplorationRoamState(null));
    }

    public override void _Process(float delta)
    {
        dialogDelayTimer -= delta;

        base._Process(delta);
    }

    public override void ApplyState<T>(T newState)
    {
        if (CurrentState is DialogState)
        {
            dialogDelayTimer = DialogDelay;
        }

        base.ApplyState(newState);
    }
}
