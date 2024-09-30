using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLOutput : MonoBehaviour
{
    private StreamOutlet outlet;
    public string StreamType = "Position"; 

    private Coroutine positionCoroutine;
    private double sampleRate = LSL.LSL.IRREGULAR_RATE;

    void Start()
    {
        // Create LSL stream info and outlet
        // Refer to the LSL.cs for more info on the parameters
        StreamInfo streamInfo = new StreamInfo("UnityPositionStream", StreamType, 3, Time.fixedDeltaTime * 1000, LSL.channel_format_t.cf_float32);
        XMLElement chans = streamInfo.desc().append_child("channels");
        chans.append_child("channel").append_child_value("label", "X");
        chans.append_child("channel").append_child_value("label", "Y");
        chans.append_child("channel").append_child_value("label", "Z");

        outlet = new StreamOutlet(streamInfo);
        // positionCoroutine = StartCoroutine(PositionCoroutine());
    }

    void FixedUpdate()
    {
        // Get the position of the GameObject
        Vector3 position = transform.position;

        // Create a float array to store the position data
        float[] positionData = new float[3];
        positionData[0] = position.x;
        positionData[1] = position.y;
        positionData[2] = position.z;

        // Send the position data through the LSL outlet
        outlet.push_sample(positionData);
    }

    // IEnumerator PositionCoroutine()
    // {
    //     // Creating a new variable at each call in order to not populate the memory
    //     WaitForSeconds wfsVariable = new WaitForSeconds(sampleRate);
    //     while(true){
            
    //         Vector3 position = transform.position;
            
    //         float[] positionData = new float[3];
    //         positionData[0] = position.x;
    //         positionData[1] = position.y;
    //         positionData[2] = position.z;
            
    //         Debug.Log($"Position: {positionData[0]}, {positionData[1]}, {positionData[2]}");
    //         yield return wfsVariable;
    //     }
    // }
}
