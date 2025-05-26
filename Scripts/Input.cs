using Godot;
public static class GlobalInput
{
    public static FrameInput Prev;
    public static FrameInput Current;
    public static void UpdateInput(Vector2 direction, bool jump, bool shoot)
    {
        ButtonInput jumpButton = new ButtonInput(Current.Jump.Hold, jump);
        ButtonInput shootButton = new ButtonInput(Current.Shoot.Hold, shoot);
        Prev = Current;
        Current = new FrameInput(direction, jumpButton, shootButton);
    }
}

public record struct FrameInput(Vector2 Direction, ButtonInput Jump, ButtonInput Shoot);
public record struct ButtonInput(bool Hold, bool Press, bool Release)
{
    public ButtonInput(bool prev, bool current) : this(current, !prev && current, prev && !current)
    {

    }
}