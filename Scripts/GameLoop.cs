using System;
using Godot;
using MoonTools.ECS;
using MyECS;
using MyECS.Components;
using MyECS.Systems;

public partial class GameLoop : Node2D
{
    World World = new World();
    SystemGroup readyGroup = new SystemGroup();
    SystemGroup processGroup = new SystemGroup();
    SystemGroup drawGroup = new SystemGroup();
    LoadLevelJSON levelJSON;
    [Export] string pathToLevelJson;
    [Export] bool Debug;
    [Export] float MinJumpPower;
    [Export] float MaxJumpPower;
    [Export] float Gravity;
    [Export] float MaxFallSpeed;
    public override void _Ready()
    {
        processGroup
            .Add(new TimedComponentSystem<RetainStateTimer>(World))
            .Add(new CountUpSystem<HoldJumpTimer>(World))
            .Add(new PlayerSMSystem(World))
            // .Add(new TestPlayerInputSystem(World))
            .Add(new Motion(World))
            .Add(new RemoveComponentSystem<NewlySpawned>(World))
        ;

        drawGroup
        .Add(new DrawTilemapSystem(World, this))
        .Add(new DrawColoredRectSystem(World, this))
        ;
        EntityPrefabs.Init(World);
        // EntityPrefabs.CreateTestPlayer(new Vector2(30, 40));
        levelJSON = new LoadLevelJSON(World, new Vector2I(16, 16));
        levelJSON.ReadFile(FilePathUtils.GlobalizePath(pathToLevelJson));
        levelJSON.ReadLevel(0);
        // LevelInfo.tilemap = new CustomTilemap.Tilemap(100, new Vector2I(16, 16));
        // LevelInfo.tilemap.Resize(10, 10);
        // for (int i = 0; i < 10; i++)
        // {
        //     SetTileToFull(i, 0);
        //     SetTileToFull(i, 9);
        //     SetTileToFull(0, i);
        //     SetTileToFull(9, i);
        // }
    }
    void SetTileToFull(int x, int y)
    {
        int id = LevelInfo.tilemap.GetIndex(x, y);
        LevelInfo.tilemap.Tiles[id] = TileType.Full;
    }
    public override void _Process(double delta)
    {
        if (Debug)
        {
            Consts.MIN_JUMP_POWER = MinJumpPower;
            Consts.MAX_JUMP_POWER = MaxJumpPower;
            Consts.GRAVITY = Gravity;
            Consts.MAX_FALL_SPEED = MaxFallSpeed;
        }
        UpdateInput();
        processGroup.Run(MathUtils.DeltaToTimeSpan(delta));
        QueueRedraw();
    }
    public override void _Draw()
    {
        drawGroup.Run(new TimeSpan());
    }
    void UpdateInput()
    {
        GlobalInput.UpdateInput(new Vector2(Input.GetAxis("move_left", "move_right"), Input.GetAxis("move_up", "move_down")), IsKeyPressed("jump"), IsKeyPressed("shoot"));
    }
    bool IsKeyPressed(string key)
    {
        return Input.GetActionRawStrength(key) > 0.5f;
    }
}
