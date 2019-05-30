using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveCubeSpawner : MonoBehaviour
{

    public Cube cubePrefab;
    
    // Update is called once per frame
    void Update()
    {
        Cube go = Instantiate(cubePrefab, this.transform.position, Quaternion.identity);
        
        go.OnPooledObjectStart();
        
        Destroy(go.gameObject, 2.5f);
    }
}
