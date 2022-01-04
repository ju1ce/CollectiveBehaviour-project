using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public struct FishPositionAndId
{
    public FishPositionAndId(float3 position, int id)
    {
        Position = position;
        Id = id;
    }
    
    public float3 Position { get; }
    public int Id { get; }
    
}

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class PredatorSystem : SystemBase
{
    private float _timer;
    private float _huntingTimer;
    private bool _activeTimer;
    private bool _isHunting;
    private ArrayList _deadFishIds;
    private Allocator _alloc;
    private EntityQuery _fishQuery;
    private EntityQuery _predatorQuery;
    [DeallocateOnJobCompletion] private NativeArray<Translation> _predators;
    [DeallocateOnJobCompletion] private NativeArray<Predator> _predatorData;
    [DeallocateOnJobCompletion] private NativeArray<Translation> _fishes;
    [DeallocateOnJobCompletion] private NativeArray<Entity> _fishesEntities;
    [DeallocateOnJobCompletion] private NativeArray<Fish> _fishData;
    
    private FishPositionAndId AttackIsolated(float3 position)
    {
        FishPositionAndId goal = default;
        float angularDist = float.MinValue;

        foreach (var f1 in _fishesEntities)
        {
            FishPositionAndId nn = default;
            float nn_dist = float.MaxValue;
            
            Translation translation1 = EntityManager.GetComponentData<Translation>(f1);
            Fish fishData1 = EntityManager.GetComponentData<Fish>(f1);
            
            foreach (var f2 in _fishesEntities)
            {
                
                Translation translation2 = EntityManager.GetComponentData<Translation>(f2);
                Fish fishData2 = EntityManager.GetComponentData<Fish>(f2);
                
                float dist = math.distance(translation1.Value, translation2.Value);
                if (dist > 0 && dist < nn_dist)
                {
                    nn = new FishPositionAndId(translation2.Value, fishData2.id); // f2.Value;
                    nn_dist = dist;
                }
            }

            float angle = math.acos(math.dot(math.normalize(translation1.Value), math.normalize(nn.Position)));

            if (angle > angularDist)
            {
                goal = new FishPositionAndId(translation1.Value, fishData1.id);
                angularDist = angle;
            }
        }

        return goal;
    }
    
    private FishPositionAndId AttackCenter(float3 position)
    {
        float3 center = new float3(0, 0, 0);
        
        foreach (var fish in _fishes)
        {
            center += fish.Value;
        }

        center /= _fishes.Length;

        FishPositionAndId goal = default;
        float distance = float.MaxValue;
        
        foreach (var fish in _fishesEntities)
        {
            Translation translation = EntityManager.GetComponentData<Translation>(fish);
            Fish fishData = EntityManager.GetComponentData<Fish>(fish);
            
            float dist = math.distance(translation.Value, center);

            if (dist < distance)
            {
                distance = dist;
                goal = new FishPositionAndId(translation.Value, fishData.id);
            }
        }

        return goal;
    }

    private FishPositionAndId AttackClosest(float3 position)
    {
        FishPositionAndId goal = default;
        float distance = float.MaxValue;
        
        foreach (var fish in _fishesEntities)
        {
            Translation translation = EntityManager.GetComponentData<Translation>(fish);
            Fish fishData = EntityManager.GetComponentData<Fish>(fish);

            float currentDist = math.distance(translation.Value, position);

            if (currentDist < distance)
            {
                distance = currentDist;
                goal = new FishPositionAndId(translation.Value, fishData.id);
            }
        }

        return goal;
    }

    protected override void OnStartRunning()
    {
        _timer = 0.0f;
        _huntingTimer = 0.0f;
        _activeTimer = false;
        _deadFishIds = new ArrayList();
    }
    
    private bool IsFishInHuntingRadius()
    {
        float3 currentPosition = _predators[0].Value;

        foreach (var fish in _fishes)
        {
            if (math.distance(fish.Value, currentPosition) < _predatorData[0].HuntingRadius)
            {
                if (!_activeTimer)
                {
                    _isHunting = true;
                }
                
                return true;
            }
        }

        return false;
    }

    protected override void OnUpdate()
    {
        // Debug.Log($"Is hunting: {_isHunting}");
        // Debug.Log($"Active timer: {_activeTimer}");
        // Debug.Log($"Timer: {_timer}");
        
        // Dodani usi queriji.
        _alloc = Allocator.TempJob;
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(_alloc);
        _fishQuery = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Movement>());
        _predatorQuery = GetEntityQuery(typeof(Predator), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Movement>());

        _fishes = _fishQuery.ToComponentDataArray<Translation>(_alloc);
        _fishData = _fishQuery.ToComponentDataArray<Fish>(_alloc);
        _fishesEntities = _fishQuery.ToEntityArray(_alloc);
        
        _predators = _predatorQuery.ToComponentDataArray<Translation>(_alloc);
        _predatorData = _predatorQuery.ToComponentDataArray<Predator>(_alloc);

        float deltaTime = 1f;
        float acceleration = IsFishInHuntingRadius()
            ? _predatorData[0].HuntingAcceleration
            : _predatorData[0].MaxAcceleration;
        
        if (_activeTimer)
        {
            _timer += deltaTime;
        }
        
        // Ker klicem AttackClosest znotrej tega, je treba dat withoutburst
        Entities.WithoutBurst().ForEach((ref Movement velocity, ref Predator predator,ref Rotation rotation, in Translation trans) =>
        {
            int confusabilityCount = 0;

            float distToDesiredPrey = math.distance(trans.Value, predator.HuntedFish.Position);

            if (distToDesiredPrey <= predator.CatchDistance)
            {
                for (int i = 0; i < _fishes.Length; i++)
                {
                    float dist = math.distance(trans.Value, _fishes[i].Value);
                    
                    if (dist < predator.ConfusabilityRadius)
                    {
                        confusabilityCount++;
                    }
                }
                
                double probsOfKill = confusabilityCount > 0 ? 1/confusabilityCount : 1.0;
                
                float rand = Random.value;
                
                // Debug.Log($"Random: {Random.value}");
                
                if (rand <= probsOfKill)
                {
                    _deadFishIds.Add(predator.HuntedFish.Id);
                    _activeTimer = true;
                    _isHunting = false;
                    Debug.Log($"Attack successful");
                }
                else
                {
                    // The predator stops hunting and waits for 30s in between every attack.
                    _activeTimer = true;
                    _isHunting = false;
                    Debug.Log($"Attack not successful");
                }
            }

            if (_timer > predator.RefocusTime)
            {
                _timer = 0.0f;
                _activeTimer = false;
                _isHunting = true;
            }
            
            if (_isHunting)
            {
                _huntingTimer += deltaTime;
                
                predator.HuntedFish = predator.Mode switch
                {
                    0 => AttackClosest(trans.Value),
                    1 => AttackCenter(trans.Value),
                    _ => AttackIsolated(trans.Value)
                };
                
                float3 drive = math.normalize(predator.HuntedFish.Position - trans.Value);
            
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

                // Kopirana koda iz fishsystem da se predator obraca,
                rotation.Value = quaternion.LookRotation(velocity.Linear, math.up());
                rotation.Value = math.mul(rotation.Value, quaternion.Euler(0f, 1.57f, 0f));

                if (_huntingTimer >= predator.HuntingTimer)
                {
                    _isHunting = false;
                }
            }

        }).Run();
        
        Entities.WithoutBurst().ForEach((Entity entity, ref Fish fishy, ref Rotation rotation, ref Movement velocity) =>
        {
            if (_deadFishIds.Count > 0 && _deadFishIds.Contains(fishy.id))
            {
                // Debug.Log($"Dead fish id {fishy.id}");
                UIManager.instance.IncreaseCount();
                entityCommandBuffer.RemoveComponent<Fish>(entity);
                rotation.Value = math.mul(rotation.Value, quaternion.Euler(1.57f, 0f, 0f));
                velocity.Linear = 0f;
            }
        }).Run();
        
        // Debug.Log(_deadFishIds.Count);

        // Disposi da ni memory leaka.
        _fishes.Dispose();
        _fishData.Dispose();
        _predators.Dispose();
        _predatorData.Dispose();
        _fishesEntities.Dispose();
        
        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();
        _deadFishIds.Clear();
    }
}
