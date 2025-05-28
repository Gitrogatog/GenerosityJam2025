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
        Vector2I screenStart = GlobalState.CurrentRoom * Consts.ROOM_SIZE;
        Vector2I screenEnd = screenStart + Consts.ROOM_SIZE;
        screenStart = new Vector2I(Mathf.Clamp(screenStart.X, 0, tilemap.XTiles), Mathf.Clamp(screenStart.Y, 0, tilemap.YTiles));
        screenEnd = new Vector2I(Mathf.Clamp(screenEnd.X, 0, tilemap.XTiles), Mathf.Clamp(screenEnd.Y, 0, tilemap.YTiles));
        for (int x = screenStart.X; x < screenEnd.X; x++)
        {
            for (int y = screenStart.Y; y < screenEnd.Y; y++)
            {
                if (tilemap.Tiles[tilemap.GetIndex(x, y)] == TileType.Full)
                {
                    drawNode.DrawRect(new Rect2(x * tileSize.X, y * tileSize.Y, tileSize.X, tileSize.Y), Colors.Blue);
                }
            }
        }
    }
}