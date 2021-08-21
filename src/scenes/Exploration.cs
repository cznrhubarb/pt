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
        BuildActors(mapComp);
        MapUtils.RefreshObstacles(mapComp, GetEntitiesWithComponent<TileLocation>());
    }

    private void CreateSystems()
    {
        AddSystem(new FollowCameraControlSystem());
        AddSystem(new ClampToMapSystem());
        AddSystem(new DepthSortSystem());
        AddSystem(new TweenCleanupSystem());

        // Event Handling Systems
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
        var camera = FindNode("Camera") as Entity;
        RegisterExistingEntity(camera);
        AddComponentToEntity(camera, new CameraWrap());
    }

    private Vector3 TilePositionFromActor(Node2D actor, Map map) =>
        map.IsoMap.PickUncovered(actor.Position)[0].GetComponent<TileLocation>().TilePosition;

    private void BuildActors(Map map)
    {
        var actor = FindNode("Trainer") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 10 },
            new SpriteWrap(),
            new Directionality() { Direction = Direction.Down },
            new Selected());

        actor = FindNode("WaTrainer") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 10 },
            new SpriteWrap(),
            new Directionality() { Direction = Direction.Down },
            new Obstacle());

        actor = FindNode("Grunkle") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new TileLocation() { TilePosition = TilePositionFromActor(actor, map), ZLayer = 3 },
            new SpriteWrap(),
            new Interactable() { Action = () => PerformHudAction("StartDialogTimeline", "TalkToARock") },
            new Obstacle());

        // TODO: Make a "trigger" object that I can visually place in the editor, but deletes the sprite during this step
        var triggers = FindNode("Triggers").GetChildren();
        foreach (var triggerObj in triggers)
        {
            var trigger = triggerObj as Trigger;
            trigger.GetChild<Sprite>(0).QueueFree();
            AddComponentsToEntity(GetNewEntity(),
                new TileLocation() { TilePosition = TilePositionFromActor(trigger, map), ZLayer = 5 },
                new WalkOnTrigger() { Action = () => TriggerCue(trigger.Cue, trigger.Params) });
        }
    }

    public void TriggerCue(CueType cueType, string[] parameters)
    {
        switch (cueType)
        {
            case CueType.StartDialog:
                StartDialog(parameters[0]);
                break;
            case CueType.ChangeScene:
                GetTree().ChangeScene($"res://src/scenes/{parameters[0]}.tscn");
                break;
            default:
                throw new ArgumentException($"Attempting to trigger unknown cue: {cueType}");
        }
    }

    public override void PerformHudAction(string actionName, params object[] args)
    {
        switch (actionName)
        {
            case "StartDialogTimeline":
                StartDialog(args[0] as string);
                break;
            default:
                throw new ArgumentException($"Attempting to perform an illegal HUD action: {actionName}");
        }
    }

    private void StartDialog(string dialogName)
    {
        if (CurrentState is ExplorationRoamState && dialogDelayTimer <= 0)
        {
            ApplyState(new DialogState());
            var dialog = DialogicSharp.Start(dialogName, false);
            AddChild(dialog);
            dialog.Connect("timeline_end", this, nameof(DialogFinished));
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
