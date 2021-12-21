using Unity.Entities;

[GenerateAuthoringComponent]
public struct Predator : IComponentData
{ 
    public float MaxAcceleration;
    public float MaxSpeed;
    public float MinSpeed;
    public float HuntingAcceleration;
    public float HuntingRadius;
}