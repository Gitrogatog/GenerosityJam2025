namespace MyECS.Systems;

using System;
using System.Collections;
using MoonTools.ECS;
using MyECS.Components;

public class PlayerSMSystem : StateMachineSystem<PlayerState>
{
    Filter PlayerFilter;
    float MaxJumpPower = 300f;
    float MinJumpPower = 200f;
    float MaxJumpTime = 2f;
    float LandTime = 1f;
    public PlayerSMSystem(World world) : base(world)
    {
        PlayerFilter = FilterBuilder.Include<PlayerState>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (Entity entity in PlayerFilter.Entities)
        {
            PlayerState state = Get<PlayerState>(entity);
            if (Has<NewlySpawned>(entity))
            {
                EnterState(entity, state);
            }
            TickState(entity, state, (float)delta.TotalSeconds);
            // Godot.GD.Print($"{state} grounded:{Has<Grounded>(entity)}");

        }
    }

    protected override void EnterState(Entity entity, PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                {
                    break;
                }
            case PlayerState.ChargeJump:
                {
                    Set(entity, new IntendedMove());
                    Set(entity, new HoldJumpTimer(MaxJumpTime));
                    break;
                }
            case PlayerState.Jump:
                {
                    Set(entity, new AttemptJumpThisFrame());
                    Set(entity, new IntendedMove(GlobalInput.Current.Direction.X));
                    break;
                }
            case PlayerState.Land:
                {
                    Set(entity, new Velocity());
                    Set(entity, new RetainStateTimer(LandTime));
                    break;
                }
        }
    }

    protected override void ExitState(Entity entity, PlayerState state)
    {

    }

    protected override void TickState(Entity entity, PlayerState state, float delta)
    {
        switch (state)
        {
            case PlayerState.Idle:
                {
                    if (GlobalInput.Current.Jump.Press && Has<Grounded>(entity))
                    {
                        ChangeState(entity, state, PlayerState.ChargeJump);
                        return;
                    }
                    Set(entity, new IntendedMove(GlobalInput.Current.Direction.X));
                    break;
                }
            case PlayerState.ChargeJump:
                {

                    if (GlobalInput.Current.Jump.Release)
                    {
                        float currentTime = Has<HoldJumpTimer>(entity) ? Get<HoldJumpTimer>(entity).Time : 0;
                        Godot.GD.Print($"curr:{currentTime} max:{MaxJumpTime} div:{currentTime / MaxJumpTime}");
                        float jumpPower = MathUtils.Lerp(Consts.MIN_JUMP_POWER, Consts.MAX_JUMP_POWER, currentTime / MaxJumpTime);
                        Godot.GD.Print($"jump powre:{jumpPower}");
                        Set(entity, new CanJump(jumpPower)); // 
                        ChangeState(entity, state, PlayerState.Jump);
                    }
                    break;
                }
            case PlayerState.Jump:
                {
                    if (Has<Grounded>(entity) && Get<Velocity>(entity).Value.Y >= 0)
                    {
                        ChangeState(entity, state, PlayerState.Land);
                        return;
                    }
                    break;
                }
            case PlayerState.Land:
                {
                    if (!Has<RetainStateTimer>(entity))
                    {
                        ChangeState(entity, state, PlayerState.Idle);
                    }
                    break;
                }
        }
    }
}

