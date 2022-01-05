using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;

    public Entity predator;
    public float3 offset;

    public float3 velocity;

    private EntityManager manager;
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if(predator == Entity.Null)
        {
            return;
        }
        Translation predatorPos = manager.GetComponentData<Translation>(predator);
        float3 newPos = predatorPos.Value + offset;

        float3 newVelocity = -transform.position + new Vector3(newPos.x, newPos.y, newPos.z);

        velocity = 0.9f * velocity + 0.1f * newVelocity;

        transform.position += new Vector3(velocity.x, velocity.y, velocity.z);

        //transform.position = 0.95f * transform.position + 0.05f * new Vector3(newPos.x,newPos.y,newPos.z);// + new Vector3(offset.x,offset.y,offset.z);
    }
}
