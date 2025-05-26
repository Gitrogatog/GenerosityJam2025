using System;
using Godot;
namespace MyECS.Components;

public readonly record struct ControlledByPlayer();
public readonly record struct MoveSpeed(float Value);
public readonly record struct CanJump(float Value);
// public readonly record struct Position(Vector2 Value);
public readonly record struct Velocity(Vector2 Value)
{
    public Velocity(float x, float y) : this(new Vector2(x, y)) { }
}
public readonly record struct IntendedMove(float Value);
public readonly record struct Grounded();
public readonly record struct CheckForGround(Rectangle Box);
public readonly record struct Solid();
public readonly record struct CollidesWithSolids();
public readonly record struct CanInteract();
public readonly record struct CanBeGrounded();
public readonly record struct CheckForStaticCollisions();
public readonly record struct SolidCollision();
public readonly record struct Gravity(float Value);
public readonly record struct DidJumpThisFrame();
public readonly record struct Pause();

public readonly record struct ColoredRect(Vector2 Size, Color Color);
public readonly record struct AttemptJumpThisFrame();
public readonly record struct HoldingJump();
public readonly record struct HoldJumpTimer(float Time, float Max) : TimeToMaxComponent<HoldJumpTimer>
{
    public HoldJumpTimer(float max) : this(0, max) { }
    public HoldJumpTimer Update(float t)
    {
        return new HoldJumpTimer(t, Max);
    }
}
public readonly record struct RetainStateTimer(float Time) : TimedComponent<RetainStateTimer>
{
    public RetainStateTimer Update(float t)
    {
        return new RetainStateTimer(t);
    }
}
public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public Rectangle(int width, int height) : this(-width / 2, -height / 2, width, height) { }
    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;

    public bool Intersects(Rectangle other)
    {
        return
            other.Left < Right &&
            Left < other.Right &&
            other.Top < Bottom &&
            Top < other.Bottom;
    }

    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        var x = int.Min(a.X, a.X);
        var y = int.Min(a.Y, b.Y);
        return new Rectangle(
            x,
            y,
            int.Max(a.Right, b.Right) - x,
            int.Max(a.Bottom, b.Bottom) - y
        );
    }

    public Rectangle Inflate(int horizontal, int vertical)
    {
        return new Rectangle(
            X - horizontal,
            Y - vertical,
            Width + horizontal * 2,
            Height + vertical * 2
        );
    }
}
public readonly record struct NewlySpawned();

