using Godot;
using MoonTools.ECS;
using MyECS.Components;

public static class EntityPrefabs
{
    static Prefabs prefabs;
    public static void Init(World World)
    {
        prefabs = new Prefabs(World);
    }
    public static Entity CreatePlayer(Vector2 position)
    {
        return prefabs.CreatePlayer(position);
    }
    public static Entity CreateTestPlayer(Vector2 position)
    {
        return prefabs.CreateTestPlayer(position);
    }
}

public class Prefabs : Manipulator
{

    public Prefabs(World world) : base(world)
    {
    }

    public Entity CreatePlayer(Vector2 position)
    {
        Entity player = CreateEntity();
        Set(player, new Position(position));
        Set(player, new Velocity());
        // Set(player, new CanM)
        Set(player, new Rectangle(10, 10));
        Set(player, new ColoredRect(new Vector2(10, 10), Colors.Beige));
        Set(player, PlayerState.Idle);
        Set(player, new Gravity(5f));
        Set(player, new MoveSpeed(40));
        Set(player, new CollidesWithSolids());
        Set(player, new NewlySpawned());
        // Set(player, new CheckForGround(Utilities.BoxFromTopLeft(-5, 5, 10, 0.1f)));
        return player;
    }
    public Entity CreateTestPlayer(Vector2 position)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(position));
        Set(entity, new Velocity());
        Set(entity, new Solid());
        Set(entity, new Rectangle(10, 10));
        Set(entity, new ColoredRect(new Vector2(10, 10), Colors.Beige));
        Set(entity, new ControlledByPlayer());
        return entity;
    }
}