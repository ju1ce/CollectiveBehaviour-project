using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;


public static class Globals
{
    public static bool ZoneSystem;
    public static bool TopoSystem;
    public static int TotalFish;
    public static bool Use3D;
    public static float timestep = 0.1f;
}


public class FishSpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject Prefab;
    public int Count;
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
    public float noise = 1f;

    public float FOV = 300;

    public bool ZoneBasedBehaviour;
    public bool TopologicalBasedBehaviour;
    public bool Use3D;

    public int topo_neighbours = 7;


    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Globals.ZoneSystem = ZoneBasedBehaviour;
        Globals.TopoSystem = TopologicalBasedBehaviour;
        Globals.TotalFish = Count * Count;
        Globals.Use3D = Use3D;

        //UIManager.instance.SetTotalFish(Count * Count);

        var spawnerData = new FishSpawnerData
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            Count = Count,
            FOV = math.cos((FOV / 2) * 0.0174533f),
            minSpeed = minSpeed,
            maxSpeed = maxSpeed,
            maxAccel = maxAccel,
            sepRadius = sepRadius,
            sepWeight = sepWeight,
            aliRadius = aliRadius,
            aliWeight = aliWeight,
            cohRadius = cohRadius,
            cohWeight = cohWeight,
            escRadius = escRadius,
            escWeight = escWeight,
            noise = noise,
            topo_neighbours = topo_neighbours

        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}