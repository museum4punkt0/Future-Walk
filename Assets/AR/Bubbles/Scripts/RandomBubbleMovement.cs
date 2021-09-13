using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBubbleMovement : MonoBehaviour
{
    private Vector3 direction;

    void Start()
    {
       direction = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f)).normalized;

       StartCoroutine(Movetwosecond());
    }

    void Update()
    {
       transform.Translate(direction * (Time.deltaTime * 0.004f));
    }

    private IEnumerator Movetwosecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            direction = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f)).normalized;

            yield return new WaitForSeconds(2);
            direction = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f)).normalized;

            yield return new WaitForSeconds(3);
            direction = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f)).normalized;

            yield return new WaitForSeconds(4);
            direction = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f)).normalized;
        }
       
    }
}
