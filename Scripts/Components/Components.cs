using Godot;
namespace MyECS.Components;

public readonly record struct Position(Vector2 Value);
public readonly record struct Velocity(Vector2 Value);
public readonly record struct CheckForStaticCollisions();
public readonly record struct SolidCollision();
public readonly record struct Pause();
