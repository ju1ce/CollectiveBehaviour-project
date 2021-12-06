using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

using UnityEngine;

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
        //float deltaTime = Time.DeltaTime;
        //Debug.Log("deltatime: " + deltaTime);


        Entities.ForEach((ref Translation translation, in Movement movement) =>
        {
            translation.Value += movement.Linear;
        }).Run();
    }
}