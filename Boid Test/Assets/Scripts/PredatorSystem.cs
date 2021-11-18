using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;

[AlwaysSynchronizeSystem]
public class PredatorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        
        float2 dir = new float2(1, 0);

        Entities.ForEach((ref PhysicsVelocity velocity, in Predator predatorData) =>
            {
                float2 newVel = velocity.Linear.xz;
                newVel += dir * predatorData.Speed * deltaTime;

                velocity.Linear.xz = newVel;
            }
        ).Run();
    }
}
