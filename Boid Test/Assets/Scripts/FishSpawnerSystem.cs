using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class FishSpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        float Start = 0.0f;
        float Step = 2f;

        Entities
            .WithBurst()
            .ForEach((Entity entity, int entityInQueryIndex, in FishSpawnerData fishSpawnerData, in LocalToWorld location) =>
            {
                uint seed = (uint)(1);
                Random rnd = new Random(seed);

                for (var x = 0; x < fishSpawnerData.Count; x++)
                {
                    for (var y = 0; y < fishSpawnerData.Count; y++)
                    {
                        Entity instance = commandBuffer.Instantiate(entityInQueryIndex, fishSpawnerData.Prefab);

                        float3 position = math.transform(location.Value, new float3(Start + x * Step, 0f, Start + y * Step)); // todo

                        //var position = math.transform(location.Value, new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));

                        commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation { Value = position });

                        quaternion rotVal = quaternion.AxisAngle(math.up(), rnd.NextFloat(1.40f, 1.74f));
                        commandBuffer.SetComponent(entityInQueryIndex, instance, new Rotation { Value = rotVal });

                        //commandBuffer.AddComponent(entityInQueryIndex, instance, new Movement());
                        commandBuffer.SetComponent(entityInQueryIndex, instance, new Movement { Linear = math.mul(rotVal, math.left()) });

                        //commandBuffer.AddComponent(entityInQueryIndex, instance, new Fish());
                        commandBuffer.SetComponent(entityInQueryIndex, instance, new Fish
                        {
                            dead = false,
                            id = x * fishSpawnerData.Count + y,
                            fov = fishSpawnerData.FOV,
                            min_speed = fishSpawnerData.minSpeed,
                            max_speed = fishSpawnerData.maxSpeed,
                            max_accel = fishSpawnerData.maxAccel,
                            sep_rad = fishSpawnerData.sepRadius,
                            sep_weight = fishSpawnerData.sepWeight,
                            ali_rad = fishSpawnerData.aliRadius,
                            ali_weight = fishSpawnerData.aliWeight,
                            coh_rad = fishSpawnerData.cohRadius,
                            coh_weight = fishSpawnerData.cohWeight,
                            esc_rad = fishSpawnerData.escRadius,
                            esc_weight = fishSpawnerData.escWeight
                        });
                    }

                }

                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
