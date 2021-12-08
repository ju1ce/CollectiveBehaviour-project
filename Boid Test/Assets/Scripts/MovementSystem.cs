using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(FishSystem))]
public partial class MovementSystem : SystemBase
{
    protected override void OnCreate()
    {
        World.GetExistingSystem<FixedStepSimulationSystemGroup>().Timestep = 0.1f;
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, in Movement movement) =>
        {
            translation.Value += movement.Linear;
        }).Run();
    }
}