using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasedScaling : MonoBehaviour
{

    public bool visible = true;

    Vector3 initialLocalScale = Vector3.one;
    Vector3 collapsedScale = Vector3.zero;

    float target = 1;
    public float collapsed = 0;
    float s = 0;
    // Start is called before the first frame update
    void Start()
    {
        initialLocalScale = transform.localScale;
    }


    // Update is called once per frame
    void Update()
    {
        if (visible) {

            target = 1;

        } else {
            target = collapsed;
        }

        s += (target - s) / 7f;
        transform.localScale = initialLocalScale * Elastic.Out(s);

    }
}

public class Elastic
{
    public static float In(float k)
    {
        if (k == 0) return 0;
        if (k == 1) return 1;
        return -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
    }

    public static float Out(float k)
    {
        if (k == 0) return 0;
        if (k == 1) return 1;
        return Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
    }

    public static float InOut(float k)
    {
        if ((k *= 2f) < 1f) return -0.5f * Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
        return Mathf.Pow(2f, -10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) * 0.5f + 1f;
    }
};
