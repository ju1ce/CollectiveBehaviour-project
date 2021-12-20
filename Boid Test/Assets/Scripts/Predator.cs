using Unity.Entities;

[GenerateAuthoringComponent]
public struct Predator : IComponentData
{
    public float Speed;

    public float MaxAcceleration;
    public float MaxSpeed;
    public float MinSpeed;
    public float HuntingAcceleration;
    public float DistanceToPrey;
}