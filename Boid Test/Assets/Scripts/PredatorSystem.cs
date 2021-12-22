using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;


[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class PredatorSystem : SystemBase
{
    private Allocator _alloc;
    private EntityQuery _fishQuery;
    private EntityQuery _predatorQuery;
    [DeallocateOnJobCompletion] private NativeArray<Translation> _predators;
    [DeallocateOnJobCompletion] private NativeArray<Predator> _predatorData;
    [DeallocateOnJobCompletion] private NativeArray<Translation> _fishes;

    private float3 _attackVector;

    private float3 AttackIsolated(float3 position)
    {
        float3 goal = default;
        float angularDist = float.MinValue;

        foreach (var f1 in _fishes)
        {
            float3 nn = default;
            float nn_dist = float.MaxValue;
            
            foreach (var f2 in _fishes)
            {
                float dist = math.distance(f1.Value, f2.Value);
                if (dist > 0 && dist < nn_dist)
                {
                    nn = f2.Value;
                    nn_dist = dist;
                }
            }

            float angle = math.acos(math.dot(math.normalize(f1.Value), math.normalize(nn)));

            if (angle > angularDist)
            {
                goal = f1.Value;
                angularDist = angle;
            }
        }
        
        return math.normalize(goal - position);
    }
    
    private float3 AttackCenter(float3 position)
    {
        float3 center = new float3(0, 0, 0);
        
        foreach (var fish in _fishes)
        {
            center += fish.Value;
        }

        center /= _fishes.Length;

        float3 goal = default;
        float distance = float.MaxValue;
        
        foreach (var fish in _fishes)
        {
            float dist = math.distance(fish.Value, center);

            if (dist < distance)
            {
                distance = dist;
                goal = fish.Value;
            }
        }
        
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
        //veèina te funkcije se ne rab veè pomoje, ker je v onupdate prestavljen

        _alloc = Allocator.TempJob;
        _fishQuery = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PhysicsVelocity>());
        _predatorQuery = GetEntityQuery(typeof(Predator), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PhysicsVelocity>());
        
        _fishes = _fishQuery.ToComponentDataArray<Translation>(_alloc);
        _predators = _predatorQuery.ToComponentDataArray<Translation>(_alloc);
        _predatorData = _predatorQuery.ToComponentDataArray<Predator>(_alloc);

        float3 currentPosition = _predators[0].Value;
        _attackVector = AttackCenter(currentPosition);
        // _attackVector = AttackClosest(currentPosition);
        // _attackVector = AttackIsolated(currentPosition);

        _fishes.Dispose();
        _predators.Dispose();
        _predatorData.Dispose();
    }
    
    private bool IsFishInHuntingRadius()
    {
        float3 currentPosition = _predators[0].Value;

        foreach (var fish in _fishes)
        {
            if (math.distance(fish.Value, currentPosition) < _predatorData[0].HuntingRadius)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnUpdate()
    {

        //dodani usi queriji
        _alloc = Allocator.TempJob;
        _fishQuery = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Movement>());
        _predatorQuery = GetEntityQuery(typeof(Predator), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Movement>());

        _fishes = _fishQuery.ToComponentDataArray<Translation>(_alloc);
        _predators = _predatorQuery.ToComponentDataArray<Translation>(_alloc);
        _predatorData = _predatorQuery.ToComponentDataArray<Predator>(_alloc);

        float deltaTime = 1f;
        float3 drive = _attackVector;
        float acceleration = IsFishInHuntingRadius()
            ? _predatorData[0].HuntingAcceleration
            : _predatorData[0].MaxAcceleration;
        
        //Ker klièem AttackClosest znotrej tega, je treba dat withoutburst
        Entities.WithoutBurst().ForEach((ref Movement velocity, ref Predator predator,ref Rotation rotation, in Translation trans) =>
        {
            //raèunanje driva usak frame za usak predator
            drive = AttackClosest(trans.Value);

            //Debug.Log("speed: " + drive);

            if (math.length(drive) > acceleration)
            {
                drive = math.normalize(drive) * acceleration;
            }
            
            velocity.Linear += drive * deltaTime;

            if(math.length(velocity.Linear) > predator.MaxSpeed)
            {
                velocity.Linear = math.normalize(velocity.Linear) * predator.MaxSpeed;
            }
            
            if (math.length(velocity.Linear) < predator.MinSpeed)
            {
                velocity.Linear = math.normalize(velocity.Linear) * predator.MinSpeed;
            }

            //kopirana koda iz fishsystem da se predator obraèa
            rotation.Value = quaternion.LookRotation(velocity.Linear, math.up());
            rotation.Value = math.mul(rotation.Value, quaternion.Euler(0f, 1.57f, 0f));

        }).Run();

        //disposi da ni memory leaka
        _fishes.Dispose();
        _predators.Dispose();
        _predatorData.Dispose();
    }

    protected override void OnDestroy()
    {
       // _fishes.Dispose();
        //_predators.Dispose();
        //_predatorData.Dispose();
    }
}
