using Unity.Entities;

[GenerateAuthoringComponent]
public struct Fish : IComponentData
{
    public int id;
    //public float direction;
    public float max_accel;
    public float min_speed;
    public float max_speed;


    public float sep_rad;
    public float sep_weight;
}