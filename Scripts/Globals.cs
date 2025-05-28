using Godot;
public static class GlobalState
{
    public static Vector2 PlayerPosition;
    public static Vector2I CurrentRoom;
}
public static class GlobalNodes
{
    public static Node2D Root;
    public static Camera2D Camera;
}

public static class Consts
{
    public static float MAX_FALL_SPEED = 200f;
    public static float MIN_JUMP_POWER;
    public static float MAX_JUMP_POWER;
    public static float GRAVITY;
    public static Vector2 SCREEN_SIZE = new Vector2(640, 360);
    public static Vector2I ROOM_SIZE = new Vector2I(20, 20);
}