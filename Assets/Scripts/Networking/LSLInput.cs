using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLInput : MonoBehaviour
{
    public string StreamType = "Position";
    public float scaleInput = 0.1f;

    StreamInfo[] streamInfos;
    StreamInlet streamInlet;

    float[] sample;
    private int channelCount = 0;

    void Update()
    {
        if (streamInlet == null)
        {
            // Resolve the LSL stream with the specified type
            streamInfos = LSL.LSL.resolve_stream("type", StreamType, 1, 0.0);
            
            if (streamInfos.Length > 0)
            {
                // Create a stream inlet for the first resolved stream
                streamInlet = new StreamInlet(streamInfos[0]);
                channelCount = streamInlet.info().channel_count();
                streamInlet.open_stream();
            }
        }

        if (streamInlet != null)
        {
            sample = new float[channelCount];
            double lastTimeStamp = streamInlet.pull_sample(sample, 0.0f);

            if (lastTimeStamp != 0.0)
            {
                // Process the received data
                Process(sample, lastTimeStamp);
            }
        }
    }

    void Process(float[] newSample, double timeStamp)
    {
        // Assuming the received data represents X, Y, and Z coordinates
        float receivedX = newSample[0];
        float receivedY = newSample[1];
        float receivedZ = newSample[2];

        // Perform actions based on the received data
        // For example, move the GameObject based on received position
        Vector3 newPosition = new Vector3(receivedX * scaleInput, receivedY * scaleInput, receivedZ * scaleInput);
        gameObject.transform.position = newPosition;
    }
}
