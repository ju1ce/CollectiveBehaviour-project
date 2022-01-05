using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(FishSystem))]
public partial class MovementSystem : SystemBase
{
    protected override void OnCreate()
    {
        World.GetExistingSystem<FixedStepSimulationSystemGroup>().Timestep = Globals.timestep;
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, in Movement movement) =>
        {
            translation.Value += movement.Linear;
        }).Run();
    }
}