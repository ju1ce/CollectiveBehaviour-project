using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : MonoBehaviour
{
    private Vector3 _moveDirection;
    private Rigidbody _rigidBody;
    
    private float speed = 1.5f;
    
    void Start()
    {
        _moveDirection = new Vector3(1.0f, 0, 0);
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _rigidBody.velocity = new Vector3(_moveDirection.x, _moveDirection.y, _moveDirection.z) * speed;
    }
}
