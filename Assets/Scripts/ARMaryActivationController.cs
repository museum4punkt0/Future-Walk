using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARMaryActivationController : ARActivationController
{
    [SerializeField] ParticleSystem particleSystem;

    protected virtual bool preActivationCheck(ARTrackedImage trackedImage)
    {
        return (trackedImage.referenceImage.name == "Mary");
    }

    protected virtual void postActivationAction()
    {
        if (particleSystem)
        {
            particleSystem.Play();
        }
    }
}
