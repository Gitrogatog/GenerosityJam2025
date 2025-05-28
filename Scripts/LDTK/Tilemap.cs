using Godot;
using MyECS.Components;
namespace CustomTilemap;
public class Tilemap
{
    int xTiles;
    int yTiles;
    int totalTiles;
    public int XTiles => xTiles;
    public int YTiles => yTiles;
    public int TotalTiles => totalTiles;
    public Rect2I Bounds => new Rect2I(0, 0, xTiles, yTiles);
    public Vector2I tileSize;
    Vector2 invSize;
    public Vector2I[] Sprites;
    public TileType[] Tiles;
    public Tilemap(int maxTiles, Vector2I tileSize)
    {
        GD.Print($"max tiles: {maxTiles} tile size {tileSize}");
        Sprites = new Vector2I[maxTiles];
        Tiles = new TileType[maxTiles];
        this.tileSize = tileSize;
        this.invSize = new Vector2(1f / tileSize.X, 1f / tileSize.Y);
    }
    public void Resize(int x, int y)
    {
        xTiles = x;
        yTiles = y;
        totalTiles = x * y;
    }
    public void ClearTilemap()
    {
        for (int i = 0; i < totalTiles; i++)
        {
            Sprites[i] = -Vector2I.One;
            Tiles[i] = TileType.Empty;
        }
    }
    public int GetIndex(int x, int y)
    {
        return (y * xTiles) + x;
    }
    public Vector2I GetXY(int idx)
    {
        return new Vector2I(idx % xTiles, idx / xTiles);
    }
    public void SetSprite(int x, int y, Vector2I spritePosOnSheet)
    {
        int id = GetIndex(x, y);
        Sprites[id] = spritePosOnSheet;
    }
    public (Vector2I, Vector2I) GetTilesWithinBox(Rectangle box)
    {
        int x = box.X / tileSize.X;
        int y = box.Y / tileSize.Y;
        int endX = box.Right / tileSize.X;
        int endY = box.Bottom / tileSize.Y;
        var scaledRect = new Rect2I(
            Mathf.Max(x, 0),
            Mathf.Max(y, 0),
            Mathf.Min(Mathf.CeilToInt(box.Right / tileSize.X) - 1, xTiles - 1),
           Mathf.Min(Mathf.CeilToInt(box.Bottom / tileSize.X) - 1, yTiles - 1)
        );
        return (
            new Vector2I(Mathf.Clamp(x, 0, xTiles - 1),
            Mathf.Clamp(y, 0, yTiles - 1)),
            new Vector2I(Mathf.Clamp(endX, 0, xTiles - 1),
            Mathf.Clamp(endY, 0, yTiles - 1))
        );
    }
    public Vector2I PosToTile(Vector2I position)
    {
        return position / tileSize;
    }
    public bool Intersect(Rectangle box)
    {
        //new Rect2(box.X, box.Y, box.Width, box.Height)
        (var start, var end) = GetTilesWithinBox(box);
        // GD.Print($"boxStart{box.X} {box.Y} findStart{start}");
        // GD.Print($"start:{start} end:{end} width:{end - start}");
        end = new Vector2I(Mathf.Max(end.X, start.X), Mathf.Max(end.Y, start.Y));
        // GD.Print($"start:{tilesInBox.Position} width:{tilesInBox.Size} end:{tilesInBox.End}");
        for (int x = start.X; x <= end.X; x++)
        {
            for (int y = start.Y; y <= end.Y; y++)
            {
                if (Tiles[GetIndex(x, y)] == TileType.Full)
                {
                    return true;
                }
            }
        }
        return false;
    }
}