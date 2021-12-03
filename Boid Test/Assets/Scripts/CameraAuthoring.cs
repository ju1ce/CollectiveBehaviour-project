using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("Custom Authoring/Camera Authoring")]
public class CameraAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject camera;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        CameraFollow cameraFollow = camera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = camera.AddComponent<CameraFollow>();
        }
        cameraFollow.predator = entity;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
