using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

public class FishSpawner : MonoBehaviour
{
    public GameObject Prefab;
    public int Count = 10;
    public float Border = 3F;
    public float minSpeed = 2F;
    public float maxSpeed = 4F;
    public float maxAccel = 2F;

    void Start()
    {
        // Create entity prefab from the game object hierarchy once
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int x = 0; x < Count; x++)
        {
            // Efficiently instantiate a bunch of entities from the already converted entity prefab
            Entity instance = entityManager.Instantiate(prefab);

            // Place the instantiated entity in a grid with some noise
            Vector3 position = transform.TransformPoint(new float3(UnityEngine.Random.Range(-Border, Border), 0F, UnityEngine.Random.Range(-Border, Border)));
            entityManager.SetComponentData(instance, new Translation { Value = position });

            quaternion rotVal = quaternion.AxisAngle(math.up(), UnityEngine.Random.Range(0F, 6F));
            entityManager.SetComponentData(instance, new Rotation { Value = rotVal});

            entityManager.AddComponentData(instance, new PhysicsVelocity());
            entityManager.SetComponentData(instance, new PhysicsVelocity { Linear = math.mul(rotVal, math.left()) });

            entityManager.AddComponentData(instance, new Fish());
            entityManager.SetComponentData(instance, new Fish
            {
                id = x,
                min_speed = minSpeed,
                max_speed = maxSpeed,
                max_accel = maxAccel,
                //direction = ,
                sep_rad = 1F,
                sep_weight = 5f,

            });
        }
    }
}