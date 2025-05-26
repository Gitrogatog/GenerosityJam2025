using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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
        foreach (ldtk.World w in jsonObject.Worlds)
        {
            foreach (Level level in w.Levels)
            {
                foreach (LayerInstance layerInstance in level.LayerInstances)
                {
                    maxTileCount = Mathf.Max(maxTileCount, (int)(layerInstance.CHei * layerInstance.CWid));
                }
            }
        }
        tilemap = new Tilemap(maxTileCount, tileSize);
        LevelInfo.tilemap = tilemap;
    }

    public void ReadLevel(int id)
    {
        // if (jsonObject.Levels.Length <= id)
        // {
        //     GD.Print("Level out of bounds!");
        // }
        // else
        {
            ReadLevel(jsonObject.Worlds[0].Levels[id]);
        }
    }
    void ReadLevel(Level level)
    {
        foreach (FieldInstance fieldInstance in level.FieldInstances)
        {

        }

        LayerInstance layerInstance = GetLayerInstanceByName(level, "Collision");

        tilemap.Resize((int)layerInstance.CWid, (int)layerInstance.CHei);
        tilemap.ClearTilemap();
        // LoadSpriteTilesFromTileLayer(layerInstance);
        LoadTilesFromIntGrid(layerInstance);

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
    void LoadTilesFromIntGrid(LayerInstance layerInstance) // loads in the collision tile data from the "Collision" int grid layer
    {
        for (int i = 0; i < layerInstance.IntGridCsv.Length; i++)
        {
            if ((int)layerInstance.IntGridCsv[i] != 0)
            {
                tilemap.Tiles[i] = TileType.Full;
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
