using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedAction : MonoBehaviour
{
    [SerializeField] UnityEvent action;


    public void ActionDelayed(int ticks)
    {
        StartCoroutine(DoDelayed(ticks));
    }

    IEnumerator DoDelayed(int ticks)
    {
        if (ticks < 0) ticks = 0;

        for(int i=0; i < ticks; i++)
        {
            yield return null;
        }

        action?.Invoke();
    }
}
