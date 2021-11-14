using Unity.Entities;

[GenerateAuthoringComponent]
public struct Predator : IComponentData
{
    public float Speed;
    public float LockOnSpeed;
}