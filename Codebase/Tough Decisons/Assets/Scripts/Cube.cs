using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour, IPooledObject
{

    public float upForce = 1f;
    public float sideForce = 0.1f;

    private Rigidbody rigBody;

    void Awake()
    {
        rigBody = GetComponent<Rigidbody>();
    }

    public void OnPooledObjectStart()
    {
        float xForce = Random.Range(-sideForce, sideForce);
        float yForce = Random.Range(upForce * 0.5f, upForce);
        float zForce = Random.Range(-sideForce, sideForce);

        rigBody.velocity = new Vector3(xForce, yForce, zForce);
    }
    
}
