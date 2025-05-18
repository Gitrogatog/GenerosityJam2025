using Godot;

namespace MyECS.Relations;

public enum CollisionDirection
{
    X, Y
}
public readonly record struct Colliding(CollisionDirection Direction, bool Solid);
public readonly record struct IgnoreSolidCollision();
public readonly record struct FollowPosition(Vector2 Offset);
public readonly record struct FollowPositionAndRotation(Vector2 Offset);