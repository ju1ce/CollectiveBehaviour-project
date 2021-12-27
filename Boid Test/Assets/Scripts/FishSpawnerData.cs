using Unity.Entities;

public struct FishSpawnerData : IComponentData
{
    public int Count;
    public Entity Prefab;

    public float Border;
    public float minSpeed;
    public float maxSpeed;
    public float maxAccel;
    public float sepWeight;
    public float sepRadius;
    public float aliWeight;
    public float aliRadius;
    public float cohWeight;
    public float cohRadius;
    public float escRadius;
    public float escWeight;
    public float noise;

    public float FOV;
}
