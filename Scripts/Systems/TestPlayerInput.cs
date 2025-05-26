namespace MyECS;
using System;
using Godot;
using MoonTools.ECS;
using MyECS.Components;

public class TestPlayerInputSystem : System
{
    public Filter EntityFilter;
    float moveSpeed = 20f;

    public TestPlayerInputSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Include<ControlledByPlayer>()
            .Build();
    }
    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            Vector2 velocity = GlobalInput.Current.Direction * moveSpeed;
            Set(entity, new Velocity(velocity));
            GD.Print($"position: {Get<Position>(entity)}");
        }
    }
}