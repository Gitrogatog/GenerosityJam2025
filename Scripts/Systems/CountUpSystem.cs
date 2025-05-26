namespace MyECS.Systems;
using System;
using MoonTools.ECS;
using MyECS.Components;
public class CountUpSystem<T> : System where T : unmanaged, TimeToMaxComponent<T>
{
    public Filter TimerFilter;

    public CountUpSystem(World world) : base(world)
    {
        TimerFilter = FilterBuilder
                        .Include<T>()
                        .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in TimerFilter.Entities)
        {
            var timer = Get<T>(entity);
            var t = timer.Time + (float)delta.TotalSeconds;
            if (t > timer.Max)
            {
                t = timer.Max;
            }
            Set<T>(entity, timer.Update(t));
        }
    }
}