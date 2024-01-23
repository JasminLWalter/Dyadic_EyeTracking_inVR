using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialEyeTracking : MonoBehaviour
{
    public bool start = false;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    private void Update()
    {
        /**if (start)
        {
            StartCoroutine(StoreValues());
            start = false;
        }**/
    }

    IEnumerator StoreValues()
    {
        DateTime startTime = DateTime.Now;

        for (int i=0; i < 60; i++ )
        {
            
        }
        DateTime endTime = DateTime.Now.AddSeconds(75);
        TimeSpan span = endTime.Subtract(startTime);
        Console.WriteLine("Time Difference (seconds): " + span.TotalSeconds);
        yield return null;
    }

    /**
    private IEnumerator StartCounter()
    {
        countDown = 10f;
        for (int i = 0; i < 60000; i++)
        {
            while (countDown >= 0)
            {
                Debug.Log(i++);
                countDown -= Time.smoothDeltaTime;
                yield return null;
            }
        }
    }
    **/
}
