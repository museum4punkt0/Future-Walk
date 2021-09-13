using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] Text text;

    public string fps;
    float fps_total;
    int counter = 0;
    int counterMax = 20;

    // Update is called once per frame
    void Update()
    {
        float fps_f = 1 / Time.unscaledDeltaTime;

        fps_total += fps_f;
        counter++;
        if (counter >= counterMax)
        {
            fps = "" + fps_total / counterMax;

            if (text)
            {
                text.text = fps;
            }

            fps_total = 0;
            counter = 0;
        }

    }
}
