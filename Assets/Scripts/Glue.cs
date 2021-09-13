using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glue : MonoBehaviour
{
    void Update()
    {
        transform.position = new Vector3(transform.parent.transform.position.x * 2, transform.position.y, transform.position.z);        
    }
}
