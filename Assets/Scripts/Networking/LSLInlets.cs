using UnityEngine;
using LSL;
using System.Collections;

public class MultiStreamDataReceiver : MonoBehaviour
{
    private string[] streamNames = { "ExperimentPhase", "TrialNumber", "TimestampsSignaler", "SignalerReady", "BoxSelectedBySignaler" };
    private StreamInlet[] streamInlets;
    private int[] channelCounts;
    private float[][] samples;
    public float sampleInterval = 0.0001f;

    void Start()
    {
        int streamCount = streamNames.Length;
        streamInlets = new StreamInlet[streamCount];
        channelCounts = new int[streamCount];
        samples = new float[streamCount][];

        StartCoroutine(ResolveAndProcessStreams());
    }


    // void Update()
    // {
    //     for (int i = 0; i < streamNames.Length; i++)
    //     {
    //         if (streamInlets[i] == null)
    //         {
    //             ResolveStream(streamNames[i], ref streamInlets[i], ref channelCounts[i]);
    //         }
    //         if (streamInlets[i] != null)
    //         {
    //             PullAndProcessSample(streamInlets[i], ref samples[i], channelCounts[i], streamNames[i]);
    //         }
    //     }
    // }
    private IEnumerator ResolveAndProcessStreams()
    {
        while (true)
        {
            for (int i = 0; i < streamNames.Length; i++)
            {
                if (streamInlets[i] == null)
                {
                    ResolveStream(streamNames[i], ref streamInlets[i], ref channelCounts[i]);
                }

                if (streamInlets[i] != null)
                {
                    PullAndProcessSample(streamInlets[i], ref samples[i], channelCounts[i], streamNames[i]);
                }
            }

            // Wait for the specified interval before repeating the loop
            yield return new WaitForSeconds(sampleInterval);
        }
    }

    private void ResolveStream(string streamName, ref StreamInlet inlet, ref int channelCount)
    {
        // Resolve the LSL stream with the specified name
        StreamInfo[] streamInfos = LSL.LSL.resolve_stream("name", streamName, 1, 0.0);

        if (streamInfos.Length > 0)
        {
            // Create a stream inlet for the first resolved stream
            inlet = new StreamInlet(streamInfos[0]);
            channelCount = inlet.info().channel_count();
            inlet.open_stream();
        }
    }

    private void PullAndProcessSample(StreamInlet inlet, ref float[] sample, int channelCount, string streamName)
    {
        if (sample == null || sample.Length != channelCount)
        {
            sample = new float[channelCount];
        }
        
        double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

        if (lastTimeStamp != 0.0)
        {
            // Process the received data
            ProcessSample(sample, lastTimeStamp, streamName);
        }
    }

    private void ProcessSample(float[] sample, double timeStamp, string streamName)
    {
        // Implement your data processing logic here
        Debug.LogWarning($"Received sample from {streamName} at {timeStamp}: {string.Join(", ", sample)}");

        // Example: Handling specific stream data
        switch (streamName)
        {
            case "ExperimentPhase":
                float experimentPhase = sample[0];
                // Handle experimentPhase data
                break;
            case "TrialNumber":
                float trialNumber = sample[0];
                // Handle trial number data
                break;
            // case "TimestampsSignaler":
            //     float timestamp = sample[0];
            //     // Handle timestamp data
            //     break;
            // case "SignalerReady":
            //     float signalerReady = sample[0];
            //     // Handle signalerReady data
            //     break;
            // case "BoxSelectedBySignaler":
            //     float boxSelected = sample[0];
            //     // Handle boxSelected data
            //     break;
        }
    }
}
