using System;
using System.Collections.Generic;
using Godot;
using MoonTools.ECS;
using MyECS.Components;
using MyECS.Relations;

public class Motion : MoonTools.ECS.System
{
    public Filter MotionFilter;
    public Filter ColliderFilter;
    public Filter StaticFilter;

    const int CellSize = 16;
    const float CellReciprocal = 1.0f / CellSize;
    Dictionary<(int, int), HashSet<Entity>> SpatialHash = new Dictionary<(int, int), HashSet<Entity>>();

    HashSet<Entity> PossibleCollisions = new HashSet<Entity>();

    public (int x, int y) Bucket(Vector2 position)
    {
        return (
            (int)MathF.Floor(position.X * CellReciprocal),
            (int)MathF.Floor(position.Y * CellReciprocal)
        );
    }

    public void Insert(Entity e)
    {
        var box = Get<AABB>(e);
        var pos = Get<Position>(e).Value;

        var minX = box.X + pos.X - box.Width * 0.5f;
        var minY = box.Y + pos.Y - box.Height * 0.5f;
        var maxX = box.X + pos.X + box.Width * 0.5f;
        var maxY = box.Y + pos.Y + box.Height * 0.5f;

        var minBucket = Bucket(new Vector2(minX, minY));
        var maxBucket = Bucket(new Vector2(maxX, maxY));

        for (int x = minBucket.x; x <= maxBucket.x; x++)
        {
            for (int y = minBucket.y; y <= maxBucket.y; y++)
            {
                var key = (x, y);
                if (!SpatialHash.ContainsKey(key))
                {
                    SpatialHash.Add(key, new HashSet<Entity>());
                }

                SpatialHash[key].Add(e);
            }
        }
    }

    public void Retrieve(Entity e, Vector2 pos)
    {
        PossibleCollisions.Clear();
        var box = Get<AABB>(e);

        var minX = box.X + pos.X - box.Width * 0.5f;
        var minY = box.Y + pos.Y - box.Height * 0.5f;
        var maxX = box.X + pos.X + box.Width * 0.5f;
        var maxY = box.Y + pos.Y + box.Height * 0.5f;

        var minBucket = Bucket(new Vector2(minX, minY));
        var maxBucket = Bucket(new Vector2(maxX, maxY));

        for (int x = minBucket.x; x <= maxBucket.x; x++)
        {
            for (int y = minBucket.y; y <= maxBucket.y; y++)
            {
                var key = (x, y);
                if (SpatialHash.ContainsKey(key))
                {
                    PossibleCollisions.UnionWith(SpatialHash[key]);
                }
            }
        }
    }


    public Motion(World world) : base(world)
    {
        MotionFilter = FilterBuilder.Include<Velocity>().Include<Position>().Build();
        ColliderFilter = FilterBuilder.Include<Position>().Include<AABB>().Build();
        StaticFilter = FilterBuilder
        .Include<Position>()
        .Include<AABB>()
        .Include<CheckForStaticCollisions>()
        .Build();
    }

    public bool Overlaps(Vector2 posA, AABB boxA, Vector2 posB, AABB boxB)
    {
        var aMinX = boxA.X + posA.X - boxA.Width * 0.5f;
        var aMinY = boxA.Y + posA.Y - boxA.Height * 0.5f;
        var aMaxX = boxA.X + posA.X + boxA.Width * 0.5f;
        var aMaxY = boxA.Y + posA.Y + boxA.Height * 0.5f;

        var bMinX = boxB.X + posB.X - boxB.Width * 0.5f;
        var bMinY = boxB.Y + posB.Y - boxB.Height * 0.5f;
        var bMaxX = boxB.X + posB.X + boxB.Width * 0.5f;
        var bMaxY = boxB.Y + posB.Y + boxB.Height * 0.5f;

        bool overlaps = aMinX <= bMaxX &&
                        aMaxX >= bMinX &&
                        aMinY <= bMaxY &&
                        aMaxY >= bMinY;

        return overlaps;
    }

    public Vector2 Sweep(Entity e, Vector2 position, Vector2 velocity, AABB AABB)
    {
        var destX = position.X + velocity.X;
        var destY = position.Y + velocity.Y;

        foreach (var other in ColliderFilter.Entities)
        {
            var otherPos = Get<Position>(other).Value;
            var otherBox = Get<AABB>(other);
            if (e != other && Overlaps(new Vector2(destX, position.Y), AABB, otherPos, otherBox))
            {
                if (
                    !Related<IgnoreSolidCollision>(other, e) &&
                    !Related<IgnoreSolidCollision>(e, other) &&
                    Has<SolidCollision>(e) &&
                    Has<SolidCollision>(other)
                    )
                {
                    destX = position.X;
                    Relate(e, other, new Colliding(CollisionDirection.X, true));
                    break;
                }
                Relate(e, other, new Colliding(CollisionDirection.X, false));
            }
        }

        Retrieve(e, new Vector2(position.X, destY));

        foreach (var other in ColliderFilter.Entities)
        {
            var otherPos = Get<Position>(other).Value;
            var otherBox = Get<AABB>(other);
            if (e != other && Overlaps(new Vector2(position.X, destY), AABB, otherPos, otherBox))
            {

                if (!Related<IgnoreSolidCollision>(other, e) &&
                    !Related<IgnoreSolidCollision>(e, other) &&
                    Has<SolidCollision>(e) &&
                    Has<SolidCollision>(other))
                {
                    destY = position.Y;
                    Relate(e, other, new Colliding(CollisionDirection.Y, true));
                    break;
                }

                Relate(e, other, new Colliding(CollisionDirection.Y, false));
            }
        }

        return new Vector2(destX, destY);
    }

    public override void Update(TimeSpan delta)
    {

        if (Some<Pause>())
            return;

        var dt = (float)delta.TotalSeconds;

        foreach (var entity in MotionFilter.Entities)
        {
            var position = Get<Position>(entity).Value;
            var velocity = Get<Velocity>(entity).Value;

            var dest = position + velocity;

            if (HasOutRelation<FollowPosition>(entity))
            {
                var held = OutRelationSingleton<FollowPosition>(entity);
                var data = GetRelationData<FollowPosition>(entity, held);
                dest = Get<Position>(held).Value + data.Offset;
                Set(entity, new Position(dest));
                continue;
            }

            if (Has<AABB>(entity))
            {
                dest = Sweep(entity, position, velocity * dt, Get<AABB>(entity));
            }
            else
            {
                dest = position + velocity * dt;
            }

            Set(entity, new Position(dest));

        }

    }

}