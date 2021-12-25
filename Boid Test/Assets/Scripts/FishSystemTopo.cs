using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System.Collections.Generic;

using UnityEngine;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class FishSystemTopo : SystemBase
{
    private EntityQuery query;
    private EntityQuery predatorQuery;

    private bool disabled = true;

    protected override void OnUpdate()
    {
        float deltaTime = 1f;
        int numNeighbours = 7;

        if (disabled)
        {
            return;
        }

        Allocator alloc = Allocator.TempJob;

        query = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Movement>());

        predatorQuery = GetEntityQuery(typeof(Predator), ComponentType.ReadOnly<Translation>());

        NativeArray<Translation> fishes = query.ToComponentDataArray<Translation>(alloc);
        NativeArray<Movement> fishVelocity = query.ToComponentDataArray<Movement>(alloc);
        NativeArray<Translation> predatorLocation = predatorQuery.ToComponentDataArray<Translation>(alloc);

        Entities.WithBurst().WithReadOnly(fishes).WithDisposeOnCompletion(fishes)
            .WithReadOnly(fishVelocity).WithDisposeOnCompletion(fishVelocity)
            .WithReadOnly(predatorLocation).WithDisposeOnCompletion(predatorLocation)
            .ForEach((Entity entity, ref Rotation rotation, ref Movement velocity, ref Fish fishy, in Translation trans) =>
            {
                float3 esc_drive = new float3(0f, 0f, 0f);

                int esc_count = 0;

                for (int i = 0; i < predatorLocation.Length; i++)
                {
                    float3 dir = predatorLocation[i].Value - trans.Value;
                    float dist = math.length(dir);
                    dir = math.normalize(dir);

                    if (dist < 1f)
                    {
                        fishy.dead = true;
                    }

                    float fov = math.dot(dir, math.normalize(velocity.Linear));
                    if (fov < fishy.fov)
                        continue;
                    if (dist <= fishy.esc_rad)
                    {
                        esc_drive += -dir * (1 - (dist / fishy.esc_rad));
                        esc_count++;
                    }
                }

                NativeList<float2> nearestFish = new NativeList<float2>(numNeighbours, Allocator.Temp);
                int maxId = 0;
                float maxVal = 0f;

                for (int i = 0; i < fishes.Length; i++)
                {

                    float3 dir = fishes[i].Value - trans.Value;
                    float dist = math.length(dir);
                    dir = math.normalize(dir);

                    float fov = math.dot(dir, math.normalize(velocity.Linear));

                    if (dist == 0)
                    {
                        continue;
                    }

                    if (maxVal == 0 && maxId < numNeighbours)
                    {
                        nearestFish.Add(new float2(i, dist));
                        maxId++;
                        if (maxId == numNeighbours)
                        {
                            for (int j = 0; j < numNeighbours; j++)
                            {
                                if (nearestFish.ElementAt(j)[1] > maxVal)
                                {
                                    maxVal = nearestFish.ElementAt(j)[1];
                                    maxId = j;
                                }
                            }
                        }
                    }
                    else if (dist < maxVal)
                    {
                        nearestFish.RemoveAt(maxId);
                        nearestFish.Add(new float2(i, dist));

                        for (int j = 0; j < numNeighbours; j++)
                        {
                            if (nearestFish.ElementAt(j)[1] > maxVal)
                            {
                                maxVal = nearestFish.ElementAt(j)[1];
                                maxId = j;
                            }
                        }
                    }
                }

                float3 nearest_drive = new float3(0f, 0f, 0f);
                for (int i = 0; i < nearestFish.Length; i++)
                {
                    nearest_drive += fishVelocity[(int)nearestFish.ElementAt(i)[0]].Linear;
                }
                nearest_drive = fishes.Length == 0 ? nearest_drive * 0f : nearest_drive / numNeighbours;

                nearestFish.Dispose();

                //calculate drive
                float3 drive = 0.5f * nearest_drive + esc_drive * fishy.esc_weight;

                if (math.length(drive) > fishy.max_accel)
                {
                    drive = math.normalize(drive) * fishy.max_accel;
                }
                velocity.Linear += drive * deltaTime;

                if (math.length(velocity.Linear) > fishy.max_speed)
                {
                    velocity.Linear = math.normalize(velocity.Linear) * fishy.max_speed;
                }
                if (math.length(velocity.Linear) < fishy.min_speed)
                {
                    velocity.Linear = math.normalize(velocity.Linear) * fishy.min_speed;
                }
                rotation.Value = quaternion.LookRotation(velocity.Linear, math.up());
                rotation.Value = math.mul(rotation.Value, quaternion.Euler(0f, 1.57f, 0f));


            }).ScheduleParallel();

    }
}