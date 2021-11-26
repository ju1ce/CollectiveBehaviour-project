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
    private Allocator _alloc;
    private EntityQuery _fishQuery;
    private EntityQuery _predatorQuery;
    private NativeArray<Translation> _predators;
    private NativeArray<Translation> _fishes;

    private float2 AttackCenter(float3 position)
    {
        float2 goal = new float2(0, 0);
        
        foreach (var fish in _fishes)
        {
            goal.x = fish.Value.x;
            goal.y = fish.Value.z;
        }

        goal.x /= _fishes.Length;
        goal.y /= _fishes.Length;

        return goal - new float2(position.x, position.z);
    }

    protected override void OnStartRunning()
    {
        _alloc = Allocator.TempJob;
    }

    protected override void OnUpdate()
    {
        _fishQuery = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PhysicsVelocity>());
        _predatorQuery = GetEntityQuery(typeof(Predator), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PhysicsVelocity>());
        
        _fishes = _fishQuery.ToComponentDataArray<Translation>(_alloc);
        _predators = _predatorQuery.ToComponentDataArray<Translation>(_alloc);

        float3 currentPosition = _predators[0].Value;
        
        Debug.Log(currentPosition);

        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref PhysicsVelocity velocity, in Predator predatorData) =>
            {
                float2 newVel = velocity.Linear.xz;
                newVel += AttackCenter(currentPosition) * predatorData.Speed * deltaTime;

                velocity.Linear.xz = newVel;
            }
        ).Run();
    }
}
