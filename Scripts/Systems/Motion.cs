using System;
using Godot;
using MoonTools.ECS;
using MyECS.Components;
using MyECS.Relations;
using MyECS.Utility;

namespace MyECS.Systems;

public class Motion : MoonTools.ECS.System
{
    Filter VelocityFilter;
    Filter InteractFilter;
    Filter SolidFilter;
    Filter CollidesWithSolidsFilter;

    SpatialHash<Entity> InteractSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);
    SpatialHash<Entity> SolidSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);

    public Motion(World world) : base(world)
    {
        VelocityFilter = FilterBuilder.Include<Position>().Include<Velocity>().Build();
        InteractFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<CanInteract>().Build();
        SolidFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<Solid>().Build();
        CollidesWithSolidsFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<CollidesWithSolids>().Build();
    }

    void ClearCanBeHeldSpatialHash()
    {
        InteractSpatialHash.Clear();
    }

    void ClearSolidSpatialHash()
    {
        SolidSpatialHash.Clear();
    }

    Rectangle GetWorldRect(Position p, Rectangle r)
    {
        return new Rectangle(p.X + r.X, p.Y + r.Y, r.Width, r.Height);
    }

    enum SolidCheck
    {
        Miss, HitEntity, HitTilemap
    }

    (Entity other, SolidCheck hit) CheckSolidCollision(Entity e, Rectangle rect)
    {
        if (LevelInfo.tilemap.Intersect(rect))
        {
            return (default, SolidCheck.HitTilemap);
        }
        foreach (var (other, otherRect) in SolidSpatialHash.Retrieve(e, rect))
        {
            if (rect.Intersects(otherRect))
            {
                return (other, SolidCheck.HitEntity);
            }
        }

        return (default, SolidCheck.Miss);
    }

    Position SweepTest(Entity e, float dt)
    {
        var velocity = Get<Velocity>(e);
        var position = Get<Position>(e);
        var r = Get<Rectangle>(e);

        var movement = new Vector2(velocity.Value.X, velocity.Value.Y) * dt;
        var targetPosition = position + movement;

        var xEnum = new IntegerEnumerator(position.X, targetPosition.X);
        var yEnum = new IntegerEnumerator(position.Y, targetPosition.Y);

        int mostRecentValidXPosition = position.X;
        int mostRecentValidYPosition = position.Y;

        SolidCheck xHit = SolidCheck.Miss;
        SolidCheck yHit = SolidCheck.Miss;

        foreach (var x in xEnum)
        {
            var newPos = new Position(x, position.Y);
            var rect = GetWorldRect(newPos, r);

            (var other, var hit) = CheckSolidCollision(e, rect);

            xHit = hit;

            if (xHit != SolidCheck.Miss && Has<CollidesWithSolids>(e)) //Has<Solid>(other) &&
            {
                movement.X = mostRecentValidXPosition - position.X;
                position = position.SetX(position.X); // truncates x coord
                break;
            }

            mostRecentValidXPosition = x;
        }

        foreach (var y in yEnum)
        {
            var newPos = new Position(mostRecentValidXPosition, y);
            var rect = GetWorldRect(newPos, r);

            (var other, var hit) = CheckSolidCollision(e, rect);
            yHit = hit;

            if (yHit != SolidCheck.Miss && Has<CollidesWithSolids>(e)) // && Has<Solid>(other)
            {
                movement.Y = mostRecentValidYPosition - position.Y;
                position = position.SetY(position.Y); // truncates y coord
                break;
            }

            mostRecentValidYPosition = y;
        }

        return position + movement;
    }

    public override void Update(TimeSpan delta)
    {
        ClearCanBeHeldSpatialHash();
        ClearSolidSpatialHash();

        foreach (var entity in InteractFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rect = Get<Rectangle>(entity);

            InteractSpatialHash.Insert(entity, GetWorldRect(position, rect));
        }

        foreach (var entity in InteractFilter.Entities)
        {
            foreach (var other in OutRelations<Colliding>(entity))
            {
                Unrelate<Colliding>(entity, other);
            }
        }

        foreach (var entity in InteractFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rect = GetWorldRect(position, Get<Rectangle>(entity));

            foreach (var (other, otherRect) in InteractSpatialHash.Retrieve(rect))
            {
                if (rect.Intersects(otherRect))
                {
                    Relate(entity, other, new Colliding());
                }

            }
        }

        foreach (var entity in SolidFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rect = Get<Rectangle>(entity);
            SolidSpatialHash.Insert(entity, GetWorldRect(position, rect));
        }

        foreach (var entity in VelocityFilter.Entities)
        {

            var pos = Get<Position>(entity);
            var vel = Get<Velocity>(entity).Value;

            if (Has<AttemptJumpThisFrame>(entity))
            {
                if (Has<Grounded>(entity) && Has<CanJump>(entity))
                {
                    vel.Y = -Get<CanJump>(entity).Value;
                    Remove<Grounded>(entity);
                }

                Remove<AttemptJumpThisFrame>(entity);
            }

            if (Has<Gravity>(entity))
            {
                if (Has<Grounded>(entity))
                {
                    vel.Y = MathF.Min(vel.Y, 0);
                    Remove<Grounded>(entity); // removes grounded on all entities before checking again to see if they are grounded this frame
                }
                else
                {
                    float grav = Consts.GRAVITY; //Get<Gravity>(entity).Value;
                    vel.Y = MathF.Min(vel.Y + grav * (float)delta.TotalSeconds, Consts.MAX_FALL_SPEED);
                }

            }
            if (Has<IntendedMove>(entity) && Has<MoveSpeed>(entity))
            {
                vel.X = Get<IntendedMove>(entity).Value * Get<MoveSpeed>(entity).Value;
                Remove<IntendedMove>(entity);
            }

            if (Has<Rectangle>(entity) && Has<CollidesWithSolids>(entity))
            {
                var result = SweepTest(entity, (float)delta.TotalSeconds);
                Set(entity, result);
            }
            else
            {
                var scaledVelocity = vel * (float)delta.TotalSeconds;
                Set(entity, pos + scaledVelocity);
            }

            Set(entity, new Velocity(vel));


            // update spatial hashes

            if (Has<CanInteract>(entity))
            {
                var position = Get<Position>(entity);
                var rect = Get<Rectangle>(entity);

                InteractSpatialHash.Insert(entity, GetWorldRect(position, rect));
            }

            if (Has<Solid>(entity))
            {
                var position = Get<Position>(entity);
                var rect = Get<Rectangle>(entity);
                SolidSpatialHash.Insert(entity, GetWorldRect(position, rect));
            }
        }

        foreach (var entity in SolidFilter.Entities)
        {
            UnrelateAll<TouchingSolid>(entity);
        }

        foreach (var entity in CollidesWithSolidsFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rectangle = Get<Rectangle>(entity);

            var leftPos = new Position(position.X - 1, position.Y);
            var rightPos = new Position(position.X + 1, position.Y);
            var upPos = new Position(position.X, position.Y - 1);
            var downPos = new Position(position.X, position.Y + 1);

            var leftRectangle = GetWorldRect(leftPos, rectangle);
            var rightRectangle = GetWorldRect(rightPos, rectangle);
            var upRectangle = GetWorldRect(upPos, rectangle);
            var downRectangle = GetWorldRect(downPos, rectangle);

            var (leftOther, leftCollided) = CheckSolidCollision(entity, leftRectangle);
            var (rightOther, rightCollided) = CheckSolidCollision(entity, rightRectangle);
            var (upOther, upCollided) = CheckSolidCollision(entity, upRectangle);
            var (downOther, downCollided) = CheckSolidCollision(entity, downRectangle);

            if (leftCollided == SolidCheck.HitEntity)
            {
                Relate(entity, leftOther, new TouchingSolid());
            }

            if (rightCollided == SolidCheck.HitEntity)
            {
                Relate(entity, rightOther, new TouchingSolid());
            }

            if (upCollided == SolidCheck.HitEntity)
            {
                Relate(entity, upOther, new TouchingSolid());
            }
            if (downCollided == SolidCheck.HitEntity)
            {
                Relate(entity, downOther, new TouchingSolid());
            }
            if (upCollided != SolidCheck.Miss && Has<Velocity>(entity))
            {
                Vector2 velocity = Get<Velocity>(entity).Value;
                if (velocity.Y < 0)
                {
                    Set(entity, new Velocity(velocity.X, 0));
                }
            }
            if (downCollided != SolidCheck.Miss && Has<Gravity>(entity))
            {
                Set(entity, new Grounded());
            }
        }
    }
}