using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Xml;
using CustomTilemap;
using Godot;
using ldtk;
using MoonTools.ECS;
using MyECS.Components;
public class LoadLevelJSON
{
    MoonTools.ECS.World World;
    // LDTKJsonObject jsonObject;
    LdtkJson jsonObject;
    Vector2I tileSize;
    Tilemap tilemap;
    string[] tileIDToSlopeName;
    Vector2I[] tileIDToSlopeTile;
    Dictionary<string, Vector2I> slopeNameToTile = new Dictionary<string, Vector2I>();
    List<Vector2I> worldBounds = new List<Vector2I>();

    int maxTileCount;
    int slopeTileSourceID;
    public LoadLevelJSON(MoonTools.ECS.World world, Vector2I tileSize)
    {
        World = world;
        this.tileSize = tileSize;
    }
    public void ReadFile(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            GD.Print("No such file: " + filePath);
            return;
        }
        string contents = File.ReadAllText(filePath);

        jsonObject = LdtkJson.FromJson(contents);
        GD.Print($" levels count: {jsonObject.Levels.Length}");
        InitTilemap();

    }
    public void InitTilemap()
    {
        maxTileCount = 0;
        int maxWidth = 0;
        int maxHeight = 0;
        Vector2I roomSizeInPixels = Consts.ROOM_SIZE * tileSize;
        foreach (ldtk.World w in jsonObject.Worlds)
        {
            int worldWidth = 0;
            int worldHeight = 0;
            foreach (Level level in w.Levels)
            {
                Vector2I worldPos = new Vector2I((int)level.WorldX + 1, (int)level.WorldY + 1);
                Vector2I endTilePos = worldPos * Consts.ROOM_SIZE;
                worldWidth = Math.Max(endTilePos.X, maxWidth);
                worldHeight = Math.Max(endTilePos.Y, maxHeight);
                // foreach (LayerInstance layerInstance in level.LayerInstances)
                // {
                //     maxTileCount = Mathf.Max(maxTileCount, (int)(layerInstance.CHei * layerInstance.CWid));
                // }
            }
            worldBounds.Add(new Vector2I(worldWidth, worldHeight));
            maxWidth = Math.Max(maxWidth, worldWidth);
            maxHeight = Math.Max(maxHeight, worldHeight);
        }
        tilemap = new Tilemap(maxWidth * maxHeight, tileSize);
        LevelInfo.tilemap = tilemap;
    }


    public void ReadWorld(int id)
    {
        ldtk.World world = jsonObject.Worlds[id];
        tilemap.ClearTilemap();
        Vector2I bounds = worldBounds[id];
        tilemap.Resize(bounds.X, bounds.Y);
        foreach (Level level in world.Levels)
        {
            ReadLevel(level);
            GD.Print($"reading level {level.Identifier}");
        }

    }
    void ReadLevel(Level level)
    {
        foreach (FieldInstance fieldInstance in level.FieldInstances)
        {

        }

        LayerInstance layerInstance = GetLayerInstanceByName(level, "Collision");
        // LoadSpriteTilesFromTileLayer(layerInstance);
        LoadTilesFromIntGrid(layerInstance, new Vector2I((int)level.WorldX, (int)level.WorldY) / tileSize);

        layerInstance = GetLayerInstanceByName(level, "Entities");
        foreach (EntityInstance entityInstance in layerInstance.EntityInstances)
        {
            ReadEntity(entityInstance);
        }
    }
    LayerInstance GetLayerInstanceByName(Level level, string name)
    {
        foreach (LayerInstance layerInstance in level.LayerInstances)
        {
            if (layerInstance.Identifier == name)
            {
                return layerInstance;
            }
        }
        return null;
    }
    void LoadTilesFromIntGrid(LayerInstance layerInstance, Vector2I offset) // loads in the collision tile data from the "Collision" int grid layer
    {
        int roomX = Consts.ROOM_SIZE.X;
        for (int i = 0; i < layerInstance.IntGridCsv.Length; i++)
        {
            if ((int)layerInstance.IntGridCsv[i] != 0)
            {
                int x = i % roomX + offset.X;
                int y = i / roomX + offset.Y;
                tilemap.Tiles[tilemap.GetIndex(x, y)] = TileType.Full;
            }
        }
    }
    void LoadSpriteTilesFromTileLayer(LayerInstance layerInstance) // loads in the tile sprite data from the "Visuals" tile layer
    {
        foreach (var tile in layerInstance.GridTiles)
        {
            Vector2I spritePosOnSheet = new Vector2I((int)tile.Src[0], (int)tile.Src[1]);
            Vector2I tilePos = new Vector2I((int)tile.Px[0] / tileSize.X, (int)tile.Px[1] / tileSize.Y);
            int tileID = tilemap.GetIndex((int)tile.Px[0] / tileSize.X, (int)tile.Px[1] / tileSize.Y);
            tilemap.Sprites[tileID] = spritePosOnSheet;
            // tilemap.Tiles[tileID] = TileType.Empty;

            // load tile colliders from tile layer
            // int id = (int)tile.T;
            // Vector2I atlasTilePos = tileIDToSlopeTile[id];
            // gdTilemap.SetCell(0, tilePos, slopeTileSourceID, atlasTilePos);

        }
    }

    void ReadEntity(EntityInstance entityInstance)
    {
        Vector2I position = new Vector2I((int)entityInstance.Px[0], (int)entityInstance.Px[1]) + tileSize / 2;
        GD.Print($"read entity at {position}");
        switch (entityInstance.Identifier)
        {
            case "Player":
                {
                    EntityPrefabs.CreatePlayer(position);
                    break;
                }

        }
    }
}
