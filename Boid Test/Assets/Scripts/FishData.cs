using Unity.Entities;

[GenerateAuthoringComponent]
public struct Fish : IComponentData
{
    public float rotationSpeed;
    public float rotationAngleMul;
}