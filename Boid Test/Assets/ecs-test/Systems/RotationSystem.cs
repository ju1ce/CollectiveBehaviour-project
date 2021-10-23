using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public class RotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Rotation rotation, in RotationSpeedData speedData) =>
        {
            rotation.Value = math.mul(rotation.Value, quaternion.RotateX(math.radians(speedData.speed * deltaTime)));
            rotation.Value = math.mul(rotation.Value, quaternion.RotateY(math.radians(speedData.speed * deltaTime)));
            rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(math.radians(speedData.speed * deltaTime)));

        }).Run();
    }
}
