using Godot;
using Ecs;
using System.Collections.Generic;
using System;

public class Exploration : Manager
{
    public override void _Ready()
    {
        ApplyState(new ExplorationRoamState(null));
        CreateSystems();
        BuildMap();
        BuildControlElements();
        BuildActors();
    }

    private void CreateSystems()
    {
        AddSystem(new FollowCameraControlSystem());
        AddSystem(new ClampToMapSystem());
        AddSystem(new DepthSortSystem());
        AddSystem(new HandleMovementInputSystem());
        AddSystem(new TweenCleanupSystem());

        // Event Handling Systems
        AddSystem(new DeferredEventSystem());
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
        AddComponentToEntity(mapEnt, new Map(tileEntities));

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
            new Selected());
    }

    public override void PerformHudAction(string actionName, params object[] args)
    {
        switch (actionName)
        {
            default:
                throw new ArgumentException($"Attempting to perform an illegal HUD action: {actionName}");
        }
    }
}
