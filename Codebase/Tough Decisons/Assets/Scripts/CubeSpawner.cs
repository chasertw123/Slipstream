using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    void Update()
    {
        ObjectPooler.instance.SpawnFromPool("Cube", this.transform.position, Quaternion.identity);
    }
}
