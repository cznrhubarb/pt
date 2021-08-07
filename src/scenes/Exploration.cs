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
        BuildActors();
        BuildMap();
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

    private void BuildMap()
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
        MapUtils.RefreshObstacles(mapComp, GetEntitiesWithComponent<TileLocation>());

        mapNode.QueueFree();
    }

    private void BuildControlElements()
    {
        var camera = FindNode("Camera") as Entity;
        RegisterExistingEntity(camera);
        AddComponentToEntity(camera, new CameraWrap());
    }

    private void BuildActors()
    {
        var actor = FindNode("Scyther") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new TileLocation() { TilePosition = new Vector3(12, -2, 0), ZLayer = 10 },
            new SpriteWrap(),
            new Directionality() { Direction = Direction.Down },
            new Selected());

        actor = FindNode("Rock") as Entity;
        RegisterExistingEntity(actor);
        AddComponentsToEntity(actor,
            new TileLocation() { TilePosition = new Vector3(5, 1, 2), ZLayer = 3 },
            new SpriteWrap(),
            new Interactable() { Action = () => PerformHudAction("StartDialogTimeline", "TalkToARock") },
            new Obstacle());

        AddComponentsToEntity(GetNewEntity(),
            new TileLocation() { TilePosition = new Vector3(8, 2, 0), ZLayer = 5 },
            new WalkOnTrigger() { Action = () => PerformHudAction("StartDialogTimeline", "NotTheFlowers") });
        AddComponentsToEntity(GetNewEntity(),
            new TileLocation() { TilePosition = new Vector3(4, 0, 3), ZLayer = 5 },
            new WalkOnTrigger() { Action = () => GetTree().ChangeScene("res://src/scenes/Combat.tscn") });
    }

    public override void PerformHudAction(string actionName, params object[] args)
    {
        switch (actionName)
        {
            case "StartDialogTimeline":
                if (CurrentState is ExplorationRoamState && dialogDelayTimer <= 0)
                {
                    ApplyState(new DialogState());
                    var dialog = DialogicSharp.Start(args[0] as string, false);
                    AddChild(dialog);
                    dialog.Connect("timeline_end", this, nameof(DialogFinished));
                }
                break;
            default:
                throw new ArgumentException($"Attempting to perform an illegal HUD action: {actionName}");
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
