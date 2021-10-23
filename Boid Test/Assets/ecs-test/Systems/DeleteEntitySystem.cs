using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

[UpdateAfter(typeof(PickupSystem))]
public class DeleteEntitySystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities
            .WithAll<DeleteTag>()
            .ForEach((Entity entity) =>
            {
                commandBuffer.DestroyEntity(entity);
            }).Run();

        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }
}
