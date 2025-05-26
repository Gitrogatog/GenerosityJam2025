namespace MyECS;
using System;
using Godot;
using MoonTools.ECS;
using MyECS.Components;

public class RemoveComponentSystem<T> : System where T : unmanaged
{
    public Filter EntityFilter;

    public RemoveComponentSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<T>()
            .Build();
    }
    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            Remove<T>(entity);
        }
    }
}