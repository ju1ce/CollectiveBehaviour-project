using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Movement : IComponentData
{
    public float3 Linear;
}

