using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fishy : MonoBehaviour
{

    public GameObject cube;
    public float attraction;
    public float repulsion;
    public float cohesion;
    public float maxDist;
    public float speed;
    public float border;
    public Vector3 direction; 
    float time;

    GameObject[] fishes;
    

    // Start is called before the first frame update
    void Start()
    {
        time = Time.time;
        direction = new Vector3(Random.Range(0f,2f)-1f,0f,Random.Range(0f,2f)-1f);

        fishes = GameObject.FindGameObjectsWithTag("fish");
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject follow in fishes)
        {
            Vector3 moveDir = follow.transform.position - transform.position;
            float distance = moveDir.magnitude;
            if(distance < maxDist)
            {
                distance = distance/maxDist;
                moveDir = moveDir.normalized;

                Vector3 attrDir = moveDir * distance * attraction * Time.deltaTime;
                Vector3 repDir = -moveDir * (1-distance) * repulsion * Time.deltaTime;
                Vector3 cohDir = follow.transform.forward * cohesion * Time.deltaTime;

                direction = direction + attrDir + repDir + cohDir;
            }
        } 
        
        //gameObject.transform.forward = direction;
        //gameObject.transform.Rotate(direction);

        Debug.Log("dir: " + direction);

        Vector3 loc_from_origin = gameObject.transform.position - cube.transform.position;

        if(Mathf.Abs(loc_from_origin.magnitude) > border)
        {
            direction = direction - loc_from_origin.normalized * 0.01f;
        }

        direction = direction.normalized;

        gameObject.transform.rotation = Quaternion.LookRotation(direction);

        gameObject.transform.Translate(Vector3.forward * speed * Time.deltaTime);
       
        //Debug.Log(gameObject.transform.position);
    }
}
