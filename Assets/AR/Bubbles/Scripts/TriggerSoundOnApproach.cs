using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]

public class TriggerSoundOnApproach : MonoBehaviour
{
    public UnityEvent playAudioEvent;

    void Start()
    {
        this.GetComponent<Collider>().isTrigger = true;       
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) //triggering once when the screen is touched
        {
            CheckRay(Camera.main.ScreenPointToRay(Input.GetTouch(0).position));        
        }
        /* mouse clicks are interpreted as touches on Mac, that causes double triggering
        if (Input.GetMouseButtonDown(0))
        {        
            CheckRay(Camera.main.ScreenPointToRay(Input.mousePosition));
        }*/
    }

    private void CheckRay(Ray ray)
    {
        RaycastHit Hit;

        if (Physics.Raycast(ray, out Hit))
        {
            if (gameObject.name == Hit.transform.gameObject.name)
            {
                this.GetComponent<AudioSource>().Play();
                playAudioEvent.Invoke();

                Handheld.Vibrate();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
    
        if (this.enabled)
        {
            //if (other.gameObject == Camera.main.gameObject)
            this.GetComponent<AudioSource>().Play();
            playAudioEvent.Invoke();

            Handheld.Vibrate();
        }
    }
}

