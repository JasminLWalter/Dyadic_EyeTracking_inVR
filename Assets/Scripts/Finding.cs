using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finding : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("possible: " );
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            Debug.LogError("Found object: " + obj.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
