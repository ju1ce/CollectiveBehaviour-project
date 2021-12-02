using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

[UpdateBefore(typeof(FishSystem))]
public partial class FishKillSystem : SystemBase
{

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Allocator alloc = Allocator.TempJob;

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(alloc);

        Entities.ForEach((Entity entity, ref Fish fishy, ref Rotation rotation, ref PhysicsVelocity velocity) =>
        {
            if (fishy.dead)
            {
                entityCommandBuffer.RemoveComponent<Fish>(entity);
                rotation.Value = math.mul(rotation.Value, quaternion.Euler(1.57f, 0f, 0f));
                velocity.Linear = 0f;


            }
        }).Run();

        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
    }
}