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
    [DeallocateOnJobCompletion] private NativeArray<Translation> _predators;
    [DeallocateOnJobCompletion] private NativeArray<Translation> _fishes;

    private float3 AttackCenter(float3 position)
    {
        float3 goal = new float3(0, 0, 0);
        
        foreach (var fish in _fishes)
        {
            goal += fish.Value;
        }

        goal /= _fishes.Length;
        
        return math.normalize(goal - position);
    }

    private float3 AttackClosest(float3 position)
    {
        float3 goal = default;
        float distance = float.MaxValue;
        
        foreach (var fish in _fishes)
        {
            float currentDist = math.distance(fish.Value, position);

            if (currentDist < distance)
            {
                distance = currentDist;
                goal = fish.Value;
            }
        }
        
        return math.normalize(goal - position);
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
        // float3 attackVector = AttackCenter(currentPosition);
        float3 attackVector = AttackClosest(currentPosition);
        
        // Debug.Log($"Position {currentPosition}");

        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation translation, in Predator predatorData) =>
            {
                translation.Value += attackVector * predatorData.Speed * deltaTime;
            }
        ).Run();
    }

    protected override void OnDestroy()
    {
        _fishes.Dispose();
        _predators.Dispose();
    }
}
