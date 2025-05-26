namespace MyECS;
using System;
using CustomTilemap;
using Godot;
using MoonTools.ECS;
using MyECS.Components;

public class DrawTilemapSystem : System
{
    CanvasItem drawNode;

    public DrawTilemapSystem(World world, CanvasItem drawNode) : base(world)
    {
        this.drawNode = drawNode;
    }
    public override void Update(TimeSpan delta)
    {
        Tilemap tilemap = LevelInfo.tilemap;
        Vector2I tileSize = tilemap.tileSize;
        for (int x = 0; x < tilemap.XTiles; x++)
        {
            for (int y = 0; y < tilemap.YTiles; y++)
            {
                if (tilemap.Tiles[tilemap.GetIndex(x, y)] == TileType.Full)
                {
                    drawNode.DrawRect(new Rect2(x * tileSize.X, y * tileSize.Y, tileSize.X, tileSize.Y), Colors.Blue);
                }

            }
        }
    }
}