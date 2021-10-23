using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject Prefab;
    public int Count = 10;
    public float Border = 3F;
    public float RotSpeed = 3F;

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

            entityManager.AddComponentData(instance, new Fish());
            entityManager.SetComponentData(instance, new Fish { rotationSpeed = RotSpeed, rotationAngleMul = UnityEngine.Random.Range(-5f, 5F) });
        }
    }
}