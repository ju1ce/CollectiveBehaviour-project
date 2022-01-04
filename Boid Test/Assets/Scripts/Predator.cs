using Unity.Entities;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct Predator : IComponentData
{ 
    public float MaxAcceleration;
    public float MaxSpeed;
    public float MinSpeed;
    public float HuntingAcceleration;
    public float HuntingRadius;
    public float HuntingTimer;
    public float RefocusTime;
    public float ConfusabilityRadius;
    public float CatchDistance;
    // public int HuntedFishId;
    public FishPositionAndId HuntedFish;
    public int Mode;
    public bool IsHunting;
    
}