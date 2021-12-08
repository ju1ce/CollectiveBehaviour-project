using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(FishSystem))]
public partial class FishKillSystem : SystemBase
{

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Allocator alloc = Allocator.TempJob;

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(alloc);

        Entities.WithoutBurst().ForEach((Entity entity, ref Fish fishy, ref Rotation rotation, ref Movement velocity) =>
        {
            if (fishy.dead)
            {
                UIManager.instance.IncreaseCount();
                entityCommandBuffer.RemoveComponent<Fish>(entity);
                rotation.Value = math.mul(rotation.Value, quaternion.Euler(1.57f, 0f, 0f));
                velocity.Linear = 0f;


            }
        }).Run();

        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
    }
}