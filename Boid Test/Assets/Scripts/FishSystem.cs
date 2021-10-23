using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class FishSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Rotation rotation, in Fish fishy) =>
            {
                rotation.Value = math.mul(
                    math.normalize(rotation.Value),
                    //quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
                    quaternion.AxisAngle(math.up() * fishy.rotationAngleMul, fishy.rotationSpeed * deltaTime));
            }).Run();
    }
}