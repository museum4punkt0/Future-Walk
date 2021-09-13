using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnteredGoldenHandZone : MonoBehaviour
{
    public GameObject NotFoundSound;

    ParticleGravityChange gravityScript;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "MainCamera")
        {
            Debug.Log("cam si in the zone");
            gravityScript=GameObject.FindGameObjectWithTag("ParticleTrail").GetComponent<ParticleGravityChange>();
            gravityScript.GravityChange(gameObject);

            NotFoundSound.SetActive(false);
        }
    }
}
