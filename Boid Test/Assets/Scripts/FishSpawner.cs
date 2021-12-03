using System.Collections;
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
    public float sepWeight = 5F;
    public float sepRadius = 5F;
    public float aliWeight = 0.3F;
    public float aliRadius = 25F;
    public float cohWeight = 0.01F;
    public float cohRadius = 100F;
    public float escRadius = 100F;
    public float escWeight = 5F;

    public float FOV = 360;

    private EntityManager entityManager;
    private Entity prefab;
    private BlobAssetStore blobAssetStore;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAssetStore = new BlobAssetStore();
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);

    }

    private void OnDestroy()
    {
        blobAssetStore.Dispose();
    }

    void Start()
    {
        // Create entity prefab from the game object hierarchy once 

        for (int x = 0; x < Count; x++)
        {
            // Efficiently instantiate a bunch of entities from the already converted entity prefab
            Entity instance = entityManager.Instantiate(prefab);

            // Place the instantiated entity in a grid with some noise
            Vector3 position = transform.TransformPoint(new float3(UnityEngine.Random.Range(-Border, Border), 0F, UnityEngine.Random.Range(-Border, Border)));
            entityManager.SetComponentData(instance, new Translation { Value = position });

            quaternion rotVal = quaternion.AxisAngle(math.up(), UnityEngine.Random.Range(-3.14F, 3.14F));
            entityManager.SetComponentData(instance, new Rotation { Value = rotVal});

            entityManager.AddComponentData(instance, new PhysicsVelocity());
            entityManager.SetComponentData(instance, new PhysicsVelocity { Linear = math.mul(rotVal, math.left()) });

            //Debug.Log(math.cos((FOV / 2) * 0.0174533f));

            entityManager.AddComponentData(instance, new Fish());
            entityManager.SetComponentData(instance, new Fish
            {
                dead = false,
                id = x,
                fov = math.cos((FOV / 2) * 0.0174533f),
                min_speed = minSpeed,
                max_speed = maxSpeed,
                max_accel = maxAccel,
                sep_rad = sepRadius,
                sep_weight = sepWeight,
                ali_rad = aliRadius,
                ali_weight = aliWeight,
                coh_rad = cohRadius,
                coh_weight = cohWeight,
                esc_rad = escRadius,
                esc_weight = escWeight
            }) ;
        }
    }
}