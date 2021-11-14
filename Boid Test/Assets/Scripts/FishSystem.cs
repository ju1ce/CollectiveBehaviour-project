using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

public partial class FishSystem : SystemBase
{
    private EntityQuery query;
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        query = GetEntityQuery(typeof(Fish), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PhysicsVelocity>());

        Allocator alloc = Unity.Collections.Allocator.TempJob;
        NativeArray<Translation> fishes = query.ToComponentDataArray<Translation>(alloc);
        NativeArray<PhysicsVelocity> fishVelocity = query.ToComponentDataArray<PhysicsVelocity>(alloc);

        Entities.WithReadOnly(fishes).WithDisposeOnCompletion(fishes)
            .WithReadOnly(fishVelocity).WithDisposeOnCompletion(fishVelocity)
            .ForEach((ref Rotation rotation, ref PhysicsVelocity velocity,in Translation trans, in Fish fishy) =>
            {
                float3 sep_drive = new float3(0f, 0f, 0f);
                float3 ali_drive = new float3(0f, 0f, 0f);
                float3 coh_drive = new float3(0f, 0f, 0f);

                int sep_count = 0;
                int ali_count = 0;
                int coh_count = 0;

                for(int i=0; i<fishes.Length; i++)
                {
                    float3 dir = fishes[i].Value - trans.Value;
                    float dist = math.length(dir);
                    dir = math.normalize(dir);

                    float fov = math.dot(dir, math.normalize(velocity.Linear));

                    if(i == fishy.id || fov < fishy.fov)
                    {
                        continue;
                    }

                    if (dist <= fishy.sep_rad)
                    {
                        sep_drive += (-dir * (1 - (dist / fishy.sep_rad)));
                        sep_count++;
                    }else if (dist <= fishy.ali_rad)
                    {
                        ali_drive += fishVelocity[i].Linear;
                        ali_count++;
                    }else if (dist <= fishy.coh_rad)
                    {
                        coh_drive += dir*dist;
                        coh_count++;
                    }
                }
                sep_drive = sep_count == 0 ? sep_drive * 0f : sep_drive / sep_count;

                ali_drive = ali_count == 0 ? ali_drive * 0f : ali_drive / ali_count;
                ali_drive -= velocity.Linear;

                coh_drive = coh_count == 0 ? coh_drive * 0f : coh_drive / coh_count;

                //calculate drive
                float3 drive = fishy.sep_weight * sep_drive + fishy.ali_weight * ali_drive + fishy.coh_weight * coh_drive;

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


            }).ScheduleParallel();

        //fishes.Dispose();
        //fishVelocity.Dispose();
    }
}