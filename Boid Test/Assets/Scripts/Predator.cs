using Unity.Entities;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct Predator : IComponentData
{ 
    public int Mode;
    public float MaxAcceleration;
    public float MaxSpeed;
    public float MinSpeed;
    public float HuntingAcceleration;
    public float HuntingRadius;
    public float RefocusTime;
    public float ConfusabilityRadius;
    public float CatchDistance;
    public FishPositionAndId HuntedFish;
}