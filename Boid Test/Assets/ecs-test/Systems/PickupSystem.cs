using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

public class PickupSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem bufferSystem;
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        bufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        TriggerJob triggerJob = new TriggerJob
        {
            speedEntities = GetComponentDataFromEntity<SpeedData>(),
            entitiesToDelete = GetComponentDataFromEntity<DeleteTag>(),
            commandBuffer = bufferSystem.CreateCommandBuffer()
        };

        return triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
    }

    private struct TriggerJob : ITriggerEventsJob
    {
        public ComponentDataFromEntity<SpeedData> speedEntities;
        [ReadOnly] public ComponentDataFromEntity<DeleteTag> entitiesToDelete;
        public EntityCommandBuffer commandBuffer;
        public void Execute(TriggerEvent triggerEvent)
        {
            if (speedEntities.HasComponent(triggerEvent.EntityA))
            {
                if (entitiesToDelete.HasComponent(triggerEvent.EntityB))
                {
                    return;
                }
                commandBuffer.AddComponent(triggerEvent.EntityB, new DeleteTag());
            }
            if (speedEntities.HasComponent(triggerEvent.EntityB))
            {
                if (entitiesToDelete.HasComponent(triggerEvent.EntityA))
                {
                    return;
                }
                commandBuffer.AddComponent(triggerEvent.EntityA, new DeleteTag());
            }
        }
    }
}
