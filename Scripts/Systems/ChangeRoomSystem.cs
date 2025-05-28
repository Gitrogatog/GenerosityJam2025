
namespace MyECS;
using System;
using Godot;
using MoonTools.ECS;
using MyECS.Components;

public class ChangeRoomSystem : System
{
    public Filter EntityFilter;

    public ChangeRoomSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Include<PlayerState>()
            .Build();
    }
    public override void Update(TimeSpan delta)
    {
        if (Some<PlayerState>())
        {
            Entity entity = GetSingletonEntity<PlayerState>();
            Vector2I position = Get<Position>(entity).Value;
            Vector2I tilePos = LevelInfo.tilemap.PosToTile(position);
            Vector2I screenPos = tilePos / Consts.ROOM_SIZE;
            if (screenPos != GlobalState.CurrentRoom)
            {
                GlobalState.CurrentRoom = screenPos;
                GlobalNodes.Camera.Position = screenPos * LevelInfo.tilemap.tileSize * Consts.ROOM_SIZE;

            }
        }
    }
}