using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

public partial class FishSystem : SystemBase
{
    /*protected override void OnStartRunning()
    {
        //base.OnCreate();
        Entities.ForEach((ref Rotation rotation, ref PhysicsVelocity velocity, in Fish fishy) =>
        {
            rotation.Value = quaternion.AxisAngle(math.up(), fishy.direction);
            velocity.Linear = math.mul(rotation.Value, math.right());
            //velocity.Linear.x *= 1f;
            //velocity.Linear.y *= 0f;
            //velocity.Linear.z *= 0f;
            //rotation.Value = math.mul(
            //    math.normalize(rotation.Value),
            //quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
            //    quaternion.AxisAngle(math.up(), fishy.direction));
        }).Run();
    }*/
    private EntityQuery query;
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        //float3[] fishPositions = [];
        // find close fishes
        //NativeArray<float3> fishPositions = new NativeArray<float3>();
        query = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>());
        //var entityManager = World.Active.EntityManager;
        //var allEntities = entityManager.GetAllEntities();
        //fishPositions[3] = new float3(1f, 1f, 1f);
        Allocator alloc = Unity.Collections.Allocator.TempJob;
        NativeArray<Translation> fishes = query.ToComponentDataArray<Translation>(alloc);
        
        //var fishes = m_Group.ToEntityArray(Unity.Collections.Allocator.TempJob);

        //arr[4]
        /*Entities.ForEach((ref Translation position, in Fish fishy) =>
        {
            //float3 pos = position.Value;
            //fishPositions[3] = new float3(2f,2f,2f);
            fishPositions.
        }).Run();//.ScheduleParallel() or sth
        return;*/
        Entities.ForEach((ref Rotation rotation, ref PhysicsVelocity velocity,in Translation trans, in Fish fishy) =>
            {
                float3 sep_drive = new float3(0f, 0f, 0f);
                int count = 0;
                for(int i=0; i<fishes.Length; i++)
                {
                    float3 dir = fishes[i].Value - trans.Value;
                    float dist = math.length(dir);
                    dir = math.normalize(dir);
                    if (i != fishy.id && dist <= fishy.sep_rad)
                    {
                        sep_drive += (-dir * (1 - (dist / fishy.sep_rad)));
                        count++;
                    }
                }
                if (count == 0)
                    sep_drive *= 0f;
                else
                    sep_drive /= count;

                //calculate drive
                float3 drive = fishy.sep_weight * sep_drive;


                if(math.length(drive) > fishy.max_accel)
                {
                    drive = math.normalize(drive) * fishy.max_accel;
                }

                velocity.Linear += drive * deltaTime;

                if(math.length(velocity.Linear) > fishy.max_speed)
                {
                    velocity.Linear = math.normalize(velocity.Linear) * fishy.max_speed;
                }
                if (math.length(velocity.Linear) < fishy.min_speed)
                {
                    velocity.Linear = math.normalize(velocity.Linear) * fishy.min_speed;
                }
                rotation.Value = quaternion.LookRotation(velocity.Linear, math.up());
                rotation.Value = math.mul(rotation.Value, quaternion.Euler(0f, 1.57f, 0f));



            }).Run();

        fishes.Dispose();
    }
}