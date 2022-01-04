using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System.Collections.Generic;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class FishSystemTopo : SystemBase
{
    private EntityQuery query;
    private EntityQuery predatorQuery;


    protected override void OnUpdate()
    {
        float deltaTime = 1f;
        int numNeighbours = 7;

        if (!Globals.TopoSystem)
        {
            return;
        }

        Allocator alloc = Allocator.TempJob;

        query = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Movement>());

        predatorQuery = GetEntityQuery(typeof(Predator), ComponentType.ReadOnly<Translation>());

        NativeArray<Translation> fishes = query.ToComponentDataArray<Translation>(alloc);
        NativeArray<Movement> fishVelocity = query.ToComponentDataArray<Movement>(alloc);
        NativeArray<Translation> predatorLocation = predatorQuery.ToComponentDataArray<Translation>(alloc);

        double cur_time = Time.ElapsedTime;
        bool use3D = Globals.Use3D;

        Entities.WithBurst().WithReadOnly(fishes).WithDisposeOnCompletion(fishes)
            .WithReadOnly(fishVelocity).WithDisposeOnCompletion(fishVelocity)
            .WithReadOnly(predatorLocation).WithDisposeOnCompletion(predatorLocation)
            .ForEach((Entity entity, ref Rotation rotation, ref Movement velocity, ref Fish fishy, in Translation trans) =>
            {
                //pseudorandom seed
                uint seed = (uint)(233 * math.abs(trans.Value.x) + 3221 * math.abs(trans.Value.z) + cur_time + 12665) + 1;
                Random random = new Random(seed);

                float3 esc_drive = new float3(0f, 0f, 0f);

                int esc_count = 0;

                for (int i = 0; i < predatorLocation.Length; i++)
                {
                    float3 dir = predatorLocation[i].Value - trans.Value;
                    float dist = math.length(dir);
                    dir = math.normalize(dir);

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

                float3 sep_drive = new float3(0f, 0f, 0f);
                float3 ali_drive = new float3(0f, 0f, 0f);
                float3 coh_drive = new float3(0f, 0f, 0f);
                float3 noise_drive = new float3(0f, 0f, 0f);

                float sep_count = 0;
                float sep_max = float.MaxValue;

                float coh_size = 0;

                for (int i = 0; i < nearestFish.Length; i++)
                {
                    float3 dir = fishes[(int)nearestFish.ElementAt(i)[0]].Value - trans.Value;
                    float dist = math.length(dir);
                    dir = math.normalize(dir);

                    ali_drive += fishVelocity[(int)nearestFish.ElementAt(i)[0]].Linear;

                    if(dist < sep_max && dist<= fishy.sep_rad)
                    {
                        sep_drive = (-dir * (1 - (dist / fishy.sep_rad)));
                        sep_max = dist;
                    }

                    coh_drive += dir * dist;
                    coh_size += dist;

                }
                ali_drive = numNeighbours == 0 ? ali_drive * 0f : ali_drive / numNeighbours;
                ali_drive -= velocity.Linear;

                sep_drive = sep_max == float.MaxValue ? sep_drive * 0f : sep_drive;

                coh_drive = numNeighbours == 0 ? coh_drive * 0f : coh_drive / numNeighbours;

                noise_drive.xz = random.NextFloat2() * 2 - new float2(1, 1);
                if (use3D)
                {
                    noise_drive.y = random.NextFloat() * 2 - 1f;
                }

                nearestFish.Dispose();

                //calculate drive
                float3 drive = fishy.noise_weight * noise_drive + fishy.sep_weight * sep_drive + fishy.ali_weight * ali_drive + fishy.coh_weight * coh_drive + esc_drive * fishy.esc_weight;

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