using System;
using System.ComponentModel;
using Godot;
using MoonTools.ECS;
using MyECS;

public partial class GameLoop : Node2D
{
    World World = new World();
    SystemGroup readyGroup = new SystemGroup();
    SystemGroup processGroup = new SystemGroup();
    SystemGroup drawGroup = new SystemGroup();
    public override void _Ready()
    {

    }
    public override void _Process(double delta)
    {
        processGroup.Run(Utilities.DeltaToTimeSpan(delta));
    }
    public override void _Draw()
    {
        drawGroup.Run(new TimeSpan());
    }
}
