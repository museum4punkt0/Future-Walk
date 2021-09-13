using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerScript : MonoBehaviour
{
    Animator animator;

    int triggerHash = Animator.StringToHash("mytrigger");
    int resettriggerHash = Animator.StringToHash("resettrigger");

    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void start()
    {
        animator.ResetTrigger(resettriggerHash);
        animator.SetTrigger(triggerHash);        
    }

    public void reset()
    {
        animator.ResetTrigger(triggerHash);
        animator.SetTrigger(resettriggerHash);
    }
}
