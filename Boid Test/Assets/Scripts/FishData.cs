using Unity.Entities;

[GenerateAuthoringComponent]
public struct Fish : IComponentData
{
    public bool dead; 

    public int id;
    public float fov;
    //public float direction;
    public float max_accel;
    public float min_speed;
    public float max_speed;

    // separation
    public float sep_rad;
    public float sep_weight;

    // alignment
    public float ali_rad;
    public float ali_weight;

    // cohesion
    public float coh_rad;
    public float coh_weight;

    // escape
    public float esc_rad;
    public float esc_weight;

    public float noise_weight;

    public int topo_neighbours;

}
